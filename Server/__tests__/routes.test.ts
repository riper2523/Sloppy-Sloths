import Fastify from 'fastify';
import { jest } from '@jest/globals';

// Mock the database connection
const mockGetMapList = jest.fn();
const mockGetMap = jest.fn();
const mockGetCurrentUser = jest.fn();
const mockGetOwner = jest.fn();
const mockAddMap = jest.fn();
const mockChangeOwner = jest.fn();

jest.unstable_mockModule('../src/postgres_connection.js', () => ({
    getPostgresDatabaseConnection: () => ({
        getMapList: mockGetMapList,
        getMap: mockGetMap,
        getCurrentUser: mockGetCurrentUser,
        getOwner: mockGetOwner,
        addMap: mockAddMap,
        changeOwner: mockChangeOwner,
    }),
}));

// Use dynamic import because we are using unstable_mockModule for ESM
const { setUpTheRoutes } = await import('../src/set_up_the_routes.js');

describe('API Routes', () => {
    let fastify;
    const mockOwner = { nick: 'TestUser' };
    const mockPath = '/App-Data/map-files/Map.map';

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

    describe('GET /maps/', () => {
        it('should return a list of maps on success', async () => {
            const mockMaps = [
                { id: 1, data: { mapName: 'Map-1', owner: mockOwner, filePath: mockPath } },
                { id: 2, data: { mapName: 'Map-2', owner: mockOwner, filePath: mockPath } },
            ];
            mockGetMapList.mockResolvedValue({ ok: true, value: mockMaps });

            const response = await fastify.inject({
                method: 'GET',
                url: '/maps/',
            });

            expect(response.statusCode).toBe(200);
            expect(JSON.parse(response.payload)).toEqual({
                maps: [
                    { mapName: 'Map-1', owner: mockOwner },
                    { mapName: 'Map-2', owner: mockOwner }
                ],
            });
        });

        it('should return an error message on failure', async () => {
            mockGetMapList.mockResolvedValue({ ok: false, error: 'Database error' });

            const response = await fastify.inject({
                method: 'GET',
                url: '/maps/',
            });

            expect(response.statusCode).toBe(200);
            expect(JSON.parse(response.payload)).toEqual({
                errMsg: 'Database error',
            });
        });
    });

    describe('GET /maps/:mapName', () => {
        it('should return a specific map on success', async () => {
            const mockMap = { id: 1, data: { mapName: 'Map-1', owner: mockOwner, filePath: mockPath } };
            mockGetMap.mockResolvedValue({ ok: true, value: mockMap });

            const response = await fastify.inject({
                method: 'GET',
                url: '/maps/Map-1',
            });

            expect(response.statusCode).toBe(200);
            expect(JSON.parse(response.payload)).toEqual({
                map: { mapName: 'Map-1', owner: mockOwner },
            }); expect(mockGetMap).toHaveBeenCalledWith('Map-1');
        });

        it('should return an error message on failure', async () => {
            mockGetMap.mockResolvedValue({ ok: false, error: 'Map not found' });

            const response = await fastify.inject({
                method: 'GET',
                url: '/maps/NonExistent',
            });

            expect(response.statusCode).toBe(404);
            expect(JSON.parse(response.payload)).toEqual({
                errMsg: 'Map not found',
            });
        });
    });

    describe('PATCH /maps/:mapName/owner', () => {
        it('should change owner successfully', async () => {
            const mockMap = { id: 1, data: { mapName: 'Map-1', owner: { nick: 'TestUser' }, filePath: mockPath } };
            const mockNewOwner = { id: 2, data: { nick: 'NewOwner' } };
            const mockUpdatedMap = { id: 1, data: { mapName: 'Map-1', owner: { nick: 'NewOwner' }, filePath: mockPath } };

            mockGetMap.mockResolvedValue({ ok: true, value: mockMap });
            mockGetOwner.mockResolvedValue({ ok: true, value: mockNewOwner });
            mockChangeOwner.mockResolvedValue({ ok: true, value: mockUpdatedMap });

            const response = await fastify.inject({
                method: 'PATCH',
                url: '/maps/Map-1/owner',
                payload: { newOwnerNick: 'NewOwner' }
            });

            expect(response.statusCode).toBe(200);
            expect(JSON.parse(response.payload)).toEqual({
                msg: 'Owner changed successfully',
                map: { mapName: 'Map-1', owner: { nick: 'NewOwner' } }
            }); expect(mockChangeOwner).toHaveBeenCalledWith(mockMap, mockNewOwner);
        });

        it('should return 404 if map not found', async () => {
            mockGetMap.mockResolvedValue({ ok: false, error: 'Map not found' });

            const response = await fastify.inject({
                method: 'PATCH',
                url: '/maps/Unknown/owner',
                payload: { newOwnerNick: 'SomeUser' }
            });

            expect(response.statusCode).toBe(404);
            expect(JSON.parse(response.payload).errMsg).toBe('Map not found');
        });

        it('should return 404 if new owner not found', async () => {
            const mockMap = { id: 1, data: { mapName: 'Map-1', owner: mockOwner, filePath: mockPath } };
            mockGetMap.mockResolvedValue({ ok: true, value: mockMap });
            mockGetOwner.mockResolvedValue({ ok: false, error: 'User not found' });

            const response = await fastify.inject({
                method: 'PATCH',
                url: '/maps/Map-1/owner',
                payload: { newOwnerNick: 'Unknown' }
            });

            expect(response.statusCode).toBe(404);
            expect(JSON.parse(response.payload).errMsg).toBe('New owner Unknown not found');
        });
    });

    describe('POST /maps/:mapName', () => {
        it('should return 400 if map name is taken', async () => {
            mockGetMap.mockResolvedValue({ ok: true, value: {} });

            const response = await fastify.inject({
                method: 'POST',
                url: '/maps/Taken-Map'
            });

            expect(response.statusCode).toBe(400);
            expect(JSON.parse(response.payload).errMsg).toBe('Map name already taken');
        });
    });
});
