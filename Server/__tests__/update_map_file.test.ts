import Fastify from 'fastify';
import { jest } from '@jest/globals';

// Mocks
const mockGetMap = jest.fn();
const mockGetCurrentUser = jest.fn();
const mockPipeline = jest.fn();
const mockCopyFile = jest.fn();
const mockUnlink = jest.fn();

// Mock database connection
jest.unstable_mockModule('../src/postgres_connection.js', () => ({
    getPostgresDatabaseConnection: () => ({
        getMap: mockGetMap,
        getCurrentUser: mockGetCurrentUser,
    }),
}));

// Mock filesystem and stream modules
jest.unstable_mockModule('node:fs/promises', () => ({
    unlink: mockUnlink,
    mkdir: jest.fn().mockResolvedValue(undefined),
    readFile: jest.fn(),
    copyFile: mockCopyFile,
}));

jest.unstable_mockModule('node:stream/promises', () => ({
    pipeline: mockPipeline,
}));

// Mock createWriteStream
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

const { setUpTheRoutes } = await import('../src/set_up_the_routes.js');

describe('PATCH /maps/:mapName/file', () => {
    let fastify;
    const mockOwner = { nick: 'TestUser' };
    const mockPath = '/App-Data/map-files/Map.map';
    const mockMap = { id: 1, data: { mapName: 'Map-1', owner: mockOwner, filePath: mockPath } };

    beforeAll(async () => {
        fastify = Fastify();
        setUpTheRoutes(fastify);
        await fastify.ready();
    });

    afterAll(async () => {
        await fastify.close();
    });

    beforeEach(() => {
        jest.clearAllMocks();
        mockGetCurrentUser.mockResolvedValue({ ok: true, value: { id: 1, data: mockOwner } });
    });

    const createMultipartPayload = (boundary: string, content: string | null) => {
        let payload = '';
        if (content !== null) {
            payload += `--${boundary}\r\n` +
                `Content-Disposition: form-data; name="mapFile"; filename="map.json"\r\n` +
                `Content-Type: application/json\r\n\r\n` +
                `${content}\r\n`;
        }
        payload += `--${boundary}--\r\n`;
        return payload;
    };

    it('should update map file successfully if owner matches', async () => {
        mockGetMap.mockResolvedValue({ ok: true, value: mockMap });
        mockPipeline.mockResolvedValue(undefined);
        mockCopyFile.mockResolvedValue(undefined);
        mockUnlink.mockResolvedValue(undefined);

        const boundary = 'boundary123';
        const payload = createMultipartPayload(boundary, '{"new":"data"}');

        const response = await fastify.inject({
            method: 'PATCH',
            url: '/maps/Map-1/file',
            headers: {
                'content-type': `multipart/form-data; boundary=${boundary}`
            },
            payload
        });

        expect(response.statusCode).toBe(200);
        expect(JSON.parse(response.payload)).toEqual({ msg: 'Map file successfully updated' });
        expect(mockGetMap).toHaveBeenCalledWith('Map-1');
        expect(mockCopyFile).toHaveBeenCalled();
    });

    it('should return 404 if map does not exist', async () => {
        mockGetMap.mockResolvedValue({ ok: false, error: 'Map not found' });

        const response = await fastify.inject({
            method: 'PATCH',
            url: '/maps/NonExistent/file',
        });

        expect(response.statusCode).toBe(404);
        expect(JSON.parse(response.payload).errMsg).toBe('Map not found');
    });

    it('should return 403 if user is not the owner', async () => {
        mockGetMap.mockResolvedValue({ ok: true, value: mockMap });
        // Mock a DIFFERENT user than the owner
        mockGetCurrentUser.mockResolvedValue({ ok: true, value: { id: 2, data: { nick: 'WrongUser' } } });

        const boundary = 'boundary456';
        const payload = createMultipartPayload(boundary, '{"new":"data"}');

        const response = await fastify.inject({
            method: 'PATCH',
            url: '/maps/Map-1/file',
            headers: {
                'content-type': `multipart/form-data; boundary=${boundary}`
            },
            payload
        });

        expect(response.statusCode).toBe(403);
        expect(JSON.parse(response.payload).errMsg).toBe('You are not the owner of this map');
    });

    it('should return 400 if no file is provided', async () => {
        mockGetMap.mockResolvedValue({ ok: true, value: mockMap });

        const response = await fastify.inject({
            method: 'PATCH',
            url: '/maps/Map-1/file',
            headers: {
                'content-type': 'multipart/form-data; boundary=empty'
            },
            payload: '--empty--\r\n'
        });

        expect(response.statusCode).toBe(400);
        expect(JSON.parse(response.payload).errMsg).toBe('No file provided');
    });

    it('should return 401 if authentication fails', async () => {
        mockGetCurrentUser.mockResolvedValue({ ok: false, error: 'Auth failed' });

        const response = await fastify.inject({
            method: 'PATCH',
            url: '/maps/Map-1/file',
        });

        expect(response.statusCode).toBe(401);
        expect(JSON.parse(response.payload).errMsg).toBe('Authentication required');
    });
});
