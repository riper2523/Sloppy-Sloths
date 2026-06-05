import Fastify from 'fastify';
import { jest } from '@jest/globals';
import { Readable } from 'node:stream';

// Mock Dependencies
const mockGetMap = jest.fn();
const mockGetCurrentUser = jest.fn();
const mockAddMap = jest.fn();
const mockGetOwner = jest.fn();

const mockReadFile = jest.fn();
const mockUnlink = jest.fn();
const mockMkdir = jest.fn();
const mockCopyFile = jest.fn();

jest.unstable_mockModule('../src/postgres_connection.js', () => ({
    getPostgresDatabaseConnection: () => ({
        getMap: mockGetMap,
        getCurrentUser: mockGetCurrentUser,
        addMap: mockAddMap,
        getOwner: mockGetOwner,
    }),
}));

jest.unstable_mockModule('node:fs/promises', () => ({
    readFile: mockReadFile,
    unlink: mockUnlink,
    mkdir: mockMkdir,
    copyFile: mockCopyFile,
}));

// We need to mock createWriteStream from node:fs as well
jest.unstable_mockModule('node:fs', () => ({
    createWriteStream: jest.fn().mockReturnValue({
        on: jest.fn().mockImplementation(function (this: any, event, cb) {
            if (event === 'finish') setTimeout(cb, 0);
            return this;
        }),
        once: jest.fn().mockReturnThis(),
        emit: jest.fn().mockReturnThis(),
        end: jest.fn().mockReturnThis(),
        write: jest.fn().mockReturnThis(),
    }),
}));

jest.unstable_mockModule('node:stream/promises', () => ({
    pipeline: jest.fn().mockResolvedValue(undefined),
}));

const { setUpTheRoutes: set_up_the_routes } = await import('../src/set_up_the_routes.js');

describe('File Transfer Routes', () => {
    let fastify: any;
    const mockUser = { id: 1, data: { nick: 'Alice' } };
    const mockMap = {
        id: 10,
        data: {
            mapName: 'Forest',
            owner: { nick: 'Alice' },
            filePath: '/data/Forest.map'
        }
    };

    beforeAll(async () => {
        fastify = Fastify();
        set_up_the_routes(fastify);
        await fastify.ready();
    });

    afterAll(async () => {
        await fastify.close();
    });

    beforeEach(() => {
        jest.clearAllMocks();
        mockGetCurrentUser.mockResolvedValue({ ok: true, value: mockUser });
        mockMkdir.mockResolvedValue(undefined);
    });

    describe('GET /maps/:mapName/file', () => {
        it('should return file content on success', async () => {
            mockGetMap.mockResolvedValue({ ok: true, value: mockMap });
            mockReadFile.mockResolvedValue('map-content-json');

            const response = await fastify.inject({
                method: 'GET',
                url: '/maps/Forest/file',
            });

            expect(response.statusCode).toBe(200);
            expect(response.payload).toBe('map-content-json');
            expect(mockReadFile).toHaveBeenCalledWith('/data/Forest.map', 'utf-8');
        });

        it('should return 404 if map not in database', async () => {
            mockGetMap.mockResolvedValue({ ok: false, error: 'Map not found' });

            const response = await fastify.inject({
                method: 'GET',
                url: '/maps/Ghost/file',
            });

            expect(response.statusCode).toBe(404);
            expect(JSON.parse(response.payload).errMsg).toBe('Map not found');
        });
    });

    describe('POST /maps/:mapName', () => {
        it('should upload map and register in DB', async () => {
            mockGetMap.mockResolvedValue({ ok: false }); // Map doesn't exist yet
            mockAddMap.mockResolvedValue({ ok: true });

            // Create a fake multipart form request
            const boundary = '----Boundary';
            const payload = [
                `--${boundary}`,
                'Content-Disposition: form-data; name="nick"',
                '',
                'Alice',
                `--${boundary}`,
                'Content-Disposition: form-data; name="mapFile"; filename="forest.map"',
                'Content-Type: application/json',
                '',
                '{"nodes": []}',
                `--${boundary}--`,
                ''
            ].join('\r\n');

            const response = await fastify.inject({
                method: 'POST',
                url: '/maps/NewForest',
                headers: {
                    'content-type': `multipart/form-data; boundary=${boundary}`
                },
                payload
            });

            expect(response.statusCode).toBe(200);
            expect(JSON.parse(response.payload).msg).toContain('successfully uploaded');
            expect(mockAddMap).toHaveBeenCalledWith('NewForest', mockUser, expect.stringContaining('NewForest.map'));
            expect(mockCopyFile).toHaveBeenCalled();
        });

        it('should return 400 if map name is taken', async () => {
            mockGetMap.mockResolvedValue({ ok: true });

            const response = await fastify.inject({
                method: 'POST',
                url: '/maps/Existing',
            });

            expect(response.statusCode).toBe(400);
            expect(JSON.parse(response.payload).errMsg).toBe('Map name already taken');
        });
    });
});
