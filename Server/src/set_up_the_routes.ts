import type { FastifyInstance } from 'fastify';
import { getPostgresDatabaseConnection } from './postgres_connection.js';
import type { DatabaseConnection } from './database_connection.js';
import type { JsonSchemaToTsProvider } from '@fastify/type-provider-json-schema-to-ts';
import { tmpdir } from 'node:os';
import { randomUUID } from 'node:crypto';
import { join } from 'node:path';
import { createWriteStream } from 'node:fs';
import { pipeline } from 'node:stream/promises';
import { unlink, mkdir, readFile, copyFile } from 'node:fs/promises';
import fastifyMultipart from '@fastify/multipart';
import fastifyRateLimit from '@fastify/rate-limit';

const databaseConnection: DatabaseConnection = getPostgresDatabaseConnection()

const MAP_FILES_DIR = process.env.MAP_STORAGE_PATH
    ? join(process.cwd(), process.env.MAP_STORAGE_PATH)
    : join(process.cwd(), '..', 'App-Data', 'map-files');

const getMapParamsSchema = {
    type: 'object',
    properties: {
        mapName: { type: 'string', pattern: '^[a-zA-Z0-9_ -]+$' } // Path sanitization via regex
    },
    required: ['mapName']
} as const;

const ownerDataSchema = {
    type: 'object',
    properties: {
        nick: { type: 'string' }
    },
    required: ['nick']
} as const;

const mapDataSchema = {
    type: 'object',
    properties: {
        mapName: { type: 'string' },
        owner: ownerDataSchema
    },
    required: ['mapName', 'owner']
} as const;

const tmpdir_path = tmpdir();

export function setUpTheRoutes(fastifyInstance: FastifyInstance) {
    // Ensure map-files directory exists in application data folder
    fastifyInstance.addHook('onReady', async () => {
        try {
            await mkdir(MAP_FILES_DIR, { recursive: true });
        } catch (err: any) {
            fastifyInstance.log.error({ err }, `Failed to create ${MAP_FILES_DIR} directory`);
        }
    });

    // Global Error Handler
    fastifyInstance.setErrorHandler((error, request, reply) => {
        if (error.validation) {
            reply.status(400).send({ errMsg: error.message });
        } else {
            reply.status(error.statusCode || 500).send({ errMsg: error.message || "Internal Server Error" });
        }
    });

    // Global Error Handler
    fastifyInstance.setErrorHandler((error, request, reply) => {
        if (error.validation) {
            reply.status(400).send({ errMsg: error.message });
        } else {
            reply.status(error.statusCode || 500).send({ errMsg: error.message || "Internal Server Error" });
        }
    });

    // 1. GLOBAL PROTECTION: Rate Limiting
    fastifyInstance.register(fastifyRateLimit, {
        max: 100,
        timeWindow: '1 minute'
    });

    // 2. GLOBAL PROTECTION: Multipart limits
    fastifyInstance.register(fastifyMultipart, {
        limits: {
            fileSize: 5 * 1024 * 1024, // 5MB limit
            files: 1
        }
    });

    fastifyInstance.get('/status', async () => {
        return { status: 'ok' };
    });

    const fastify = fastifyInstance.withTypeProvider<JsonSchemaToTsProvider>();

    fastify.route({
        method: "GET",
        url: "/maps/",
        schema: {
            response: {
                200: {
                    type: 'object',
                    properties: {
                        maps: { type: 'array', items: mapDataSchema },
                        errMsg: { type: 'string' }
                    }
                }
            }
        },
        handler: async function (_, reply) {
            let result = await databaseConnection.getMapList()
            if (result.ok) {
                let mapList = result.value
                reply.send({ maps: mapList.map(x => ({ mapName: x.data.mapName, owner: x.data.owner })) })
            }
            else {
                // List can be empty or error
                reply.send({ errMsg: result.error })
            }
        }
    })

    fastify.route({
        method: "GET",
        url: "/maps/:mapName",
        schema: {
            params: getMapParamsSchema,
            response: {
                200: {
                    type: 'object',
                    properties: {
                        map: mapDataSchema,
                        errMsg: { type: 'string' }
                    }
                }
            }
        },
        handler: async function (request, reply) {
            const { mapName } = request.params
            let result = await databaseConnection.getMap(mapName)
            if (result.ok) {
                let mapEntity = result.value
                reply.send({ map: { mapName: mapEntity.data.mapName, owner: mapEntity.data.owner } })
            }
            else {
                // EXPLICIT 404 for missing resources
                reply.status(404).send({ errMsg: result.error })
            }
        }
    })

    // Download Map File Content
    fastify.route({
        method: "GET",
        url: "/maps/:mapName/file",
        schema: {
            params: getMapParamsSchema,
            response: {
                200: { type: 'string' }
            }
        },
        handler: async function (request, reply) {
            const { mapName } = request.params;
            const result = await databaseConnection.getMap(mapName);
            if (!result.ok) {
                return reply.status(404).send({ errMsg: result.error });
            }

            const mapEntity = result.value;
            try {
                const content = await readFile(mapEntity.data.filePath, 'utf-8');
                return reply.send(content);
            } catch (err: any) {
                return reply.status(500).send({ errMsg: `Failed to read file: ${err.message}` });
            }
        }
    })

    // Single-step upload: map file in one multipart request
    fastify.route({
        method: "POST",
        url: "/maps/:mapName",
        schema: {
            params: getMapParamsSchema,
            response: {
                200: {
                    type: 'object',
                    properties: {
                        msg: { type: 'string' }
                    },
                    required: ['msg']
                }
            }
        },
        handler: async function (request, reply) {
            const { mapName } = request.params;

            // 1. Validate Caller Identity (Source of Truth)
            const userResult = await databaseConnection.getCurrentUser();
            if (!userResult.ok) {
                return reply.status(401).send({ errMsg: "Authentication required" });
            }
            const currentUser = userResult.value;

            // 2. Pre-validation: Check if map already exists
            const exists = await databaseConnection.getMap(mapName);
            if (exists.ok) {
                return reply.status(400).send({ errMsg: "Map name already taken" });
            }

            // 3. Start receiving the multipart stream
            const data = await request.file();
            if (!data) {
                return reply.status(400).send({ errMsg: "No file provided" });
            }
            if (data.mimetype !== 'application/json') {
                data.file.resume();
                return reply.status(400).send({ errMsg: "Only JSON maps are supported" });
            }

            request.log.info({ mapName, user: currentUser.data.nick }, 'Upload request');

            const tempPath = join(tmpdir_path, `upload_${randomUUID()}.map`);
            const uniqueFileName = `${mapName}_${randomUUID()}.map`;
            const finalPath = join(MAP_FILES_DIR, uniqueFileName);

            // 3. Extract 'nick' from the form fields
            const nick = (data.fields.nick as any)?.value;
            console.log(`Upload Request: mapName=${mapName}, nick='${nick}'`);
            if (!nick) {
                data.file.resume(); // Drain stream to avoid hanging
                return reply.status(400).send({ errMsg: "Missing 'nick' field in multipart form" });
            }

            // 4. Validate Owner
            const ownerResult = await databaseConnection.getOwner(nick);
            if (!ownerResult.ok) {
                data.file.resume();
                return reply.status(404).send({ errMsg: `User ${nick} not found` });
            }

            const tempPath = join(tmpdir_path, `upload_${randomUUID()}.map`);
            const finalPath = join(MAP_FILES_DIR, `${basename(mapName)}.map`);
            let finalPathCreated = false;

            try {
                // 4. Stream directly to temp file
                await pipeline(data.file, createWriteStream(tempPath));

                // 5. Move to permanent storage (Safe for cross-device links)
                await copyFile(tempPath, finalPath);
                await unlink(tempPath).catch(() => { });

                // 6. FINAL STEP: Register in database using current user
                const dbResult = await databaseConnection.addMap(mapName, currentUser, finalPath);

                if (!dbResult.ok) {
                    await unlink(finalPath).catch(() => { });
                    return reply.status(500).send({ errMsg: dbResult.error });
                }

                return { msg: "Map successfully uploaded and registered" };

            } catch (err: any) {
                await unlink(tempPath).catch(() => { });
                return reply.status(500).send({ errMsg: err.message || "An internal server error occurred" });
            }
        }
    });

    // Update Map File Content
    fastify.route({
        method: "PATCH",
        url: "/maps/:mapName/file",
        schema: {
            params: getMapParamsSchema,
            response: {
                200: {
                    type: 'object',
                    properties: {
                        msg: { type: 'string' }
                    },
                    required: ['msg']
                }
            }
        },
        handler: async function (request, reply) {
            const { mapName } = request.params;

            // 1. Validate Caller Identity
            const userResult = await databaseConnection.getCurrentUser();
            if (!userResult.ok) {
                return reply.status(401).send({ errMsg: "Authentication required" });
            }
            const currentUser = userResult.value;

            // 2. Verify map exists
            const mapResult = await databaseConnection.getMap(mapName);
            if (!mapResult.ok) {
                return reply.status(404).send({ errMsg: mapResult.error });
            }
            const mapEntity = mapResult.value;

            // 3. Start receiving the multipart stream
            const data = await request.file();
            if (!data) {
                return reply.status(400).send({ errMsg: "No file provided" });
            }
            if (data.mimetype !== 'application/json') {
                data.file.resume();
                return reply.status(400).send({ errMsg: "Only JSON maps are supported" });
            }

            // 4. Verify Ownership against Current User
            if (mapEntity.data.owner.nick !== currentUser.data.nick) {
                data.file.resume();
                return reply.status(403).send({ errMsg: "You are not the owner of this map" });
            }

            const tempPath = join(tmpdir_path, `update_${randomUUID()}.map`);
            const finalPath = mapEntity.data.filePath;

            try {
                // 5. Stream to temp file first
                await pipeline(data.file, createWriteStream(tempPath));

                // 5. Overwrite the existing file
                await copyFile(tempPath, finalPath);
                await unlink(tempPath).catch(() => { });

                return { msg: "Map file successfully updated" };

            } catch (err: any) {
                await unlink(tempPath).catch(() => { });
                return reply.status(500).send({ errMsg: err.message || "An internal server error occurred" });
            }
        }
    });

    // changeOwner Route
    fastify.route({
        method: "PATCH",
        url: "/maps/:mapName/owner",
        schema: {
            params: getMapParamsSchema,
            body: {
                type: 'object',
                properties: {
                    newOwnerNick: { type: 'string', minLength: 1, maxLength: 255 }
                },
                required: ['newOwnerNick']
            },
            response: {
                200: {
                    type: 'object',
                    properties: {
                        msg: { type: 'string' },
                        map: mapDataSchema
                    },
                    required: ['msg', 'map']
                }
            }
        },
        handler: async function (request, reply) {
            const { mapName } = request.params;
            const { newOwnerNick } = request.body;

            // 1. Validate Caller Identity
            const userResult = await databaseConnection.getCurrentUser();
            if (!userResult.ok) {
                return reply.status(401).send({ errMsg: "Authentication required" });
            }
            const currentUser = userResult.value;

            // 2. Verify map exists
            const mapResult = await databaseConnection.getMap(mapName);
            if (!mapResult.ok) {
                return reply.status(404).send({ errMsg: mapResult.error });
            }
            const mapEntity = mapResult.value;

            // 3. Verify Ownership
            if (mapEntity.data.owner.nick !== currentUser.data.nick) {
                return reply.status(403).send({ errMsg: "You are not the owner of this map" });
            }

            // 4. Validate New Owner
            const ownerResult = await databaseConnection.getOwner(newOwnerNick);
            if (!ownerResult.ok) {
                return reply.status(404).send({ errMsg: `New owner ${newOwnerNick} not found` });
            }

            // 5. Perform Transfer
            const result = await databaseConnection.changeOwner(mapEntity, ownerResult.value);
            if (result.ok) {
                return { msg: "Owner changed successfully", map: result.value.data };
            } else {
                return reply.status(500).send({ errMsg: result.error });
            }
        }
    });
}
