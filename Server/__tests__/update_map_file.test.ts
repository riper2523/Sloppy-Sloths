import Fastify from 'fastify';
import { jest } from '@jest/globals';

// Mocks
const mockGetMap = jest.fn();
const mockPipeline = jest.fn();
const mockCopyFile = jest.fn();
const mockUnlink = jest.fn();

// Mock database connection
jest.unstable_mockModule('../src/postgres_connection.js', () => ({
  getPostgresDatabaseConnection: () => ({
    getMap: mockGetMap,
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
  createWriteStream: jest.fn().mockReturnValue({}),
}));

const { set_up_the_routes } = await import('../src/set_up_the_routes.js');

describe('PATCH /maps/:mapName/file', () => {
  let fastify;
  const mockOwner = { nick: 'TestUser' };
  const mockPath = '/App-Data/map-files/Map.map';
  const mockMap = { id: 1, data: { mapName: 'Map-1', owner: mockOwner, filePath: mockPath } };

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
  });

  const createMultipartPayload = (boundary: string, nick: string | null, content: string | null) => {
    let payload = '';
    if (nick !== null) {
      payload += `--${boundary}\r\n` +
                 `Content-Disposition: form-data; name="nick"\r\n\r\n` +
                 `${nick}\r\n`;
    }
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
    const payload = createMultipartPayload(boundary, 'TestUser', '{"new":"data"}');

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

    const boundary = 'boundary456';
    const payload = createMultipartPayload(boundary, 'WrongUser', '{"new":"data"}');

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

  it('should return 400 if nick is missing', async () => {
    mockGetMap.mockResolvedValue({ ok: true, value: mockMap });

    const boundary = 'boundary789';
    const payload = createMultipartPayload(boundary, null, '{"new":"data"}');

    const response = await fastify.inject({
      method: 'PATCH',
      url: '/maps/Map-1/file',
      headers: {
        'content-type': `multipart/form-data; boundary=${boundary}`
      },
      payload
    });

    expect(response.statusCode).toBe(400);
    expect(JSON.parse(response.payload).errMsg).toBe("Missing 'nick' field");
  });
});
