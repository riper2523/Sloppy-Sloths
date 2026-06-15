import { readFile } from 'node:fs/promises';
import { join } from 'node:path';
import pg from 'pg';
const { Client } = pg;
import { getPostgresDatabaseConnection } from '../src/postgres_connection.js';

describe('PostgresDatabase Integration Tests', () => {
    let db: any;
    let client: pg.Client;

    beforeAll(async () => {
        const testDbName = process.env.PGDATABASE || 'sloppy_sloths_test';

        // SAFETY CHECK: Ensure we are using a test database
        if (!testDbName.endsWith('_test')) {
            throw new Error(`CRITICAL: Integration tests must run on a database named '*_test' to prevent accidental data loss. Current DB: ${testDbName}`);
        }

        db = getPostgresDatabaseConnection();
        client = new Client();
        
        try {
            await client.connect();
        } catch (err: any) {
            throw new Error(`CRITICAL: PostgreSQL server is not running or unreachable. Please start your database before running integration tests. Error: ${err.message}`);
        }

        // Initialize schema
        const sqlPath = join(process.cwd(), 'database', 'schema', 'init.sql');
        const sql = await readFile(sqlPath, 'utf-8');
        
        // Clear everything first
        await client.query('DROP VIEW IF EXISTS MapsWithOwners CASCADE');
        await client.query('DROP TABLE IF EXISTS MapData CASCADE');
        await client.query('DROP TABLE IF EXISTS Owners CASCADE');
        
        // Run init script
        await client.query(sql);
    });

    afterAll(async () => {
        if (client) {
            await client.end();
        }
        // Also close the pool managed by the singleton
        if (db && db.pool) {
            await db.pool.end();
        }
    });

    beforeEach(async () => {
        // Clean tables before each test for isolation
        await client.query('TRUNCATE TABLE MapData, Owners CASCADE');
    });

    describe('User Management', () => {
        it('should add and retrieve a user', async () => {
            const addResult = await db.addUser('Alice');
            expect(addResult.ok).toBe(true);
            if (addResult.ok) {
                expect(addResult.value.data.nick).toBe('Alice');
                expect(addResult.value.id).toBeDefined();

                const getResult = await db.getOwner('Alice');
                expect(getResult.ok).toBe(true);
                if (getResult.ok) {
                    expect(getResult.value.id).toBe(addResult.value.id);
                }
            }
        });

        it('should return error when user is not found', async () => {
            const result = await db.getOwner('NonExistent');
            expect(result.ok).toBe(false);
            if (!result.ok) {
                expect(result.error).toContain('not found');
            }
        });
    });

    describe('Map Management', () => {
        it('should add a map and list it', async () => {
            // 1. Add owner
            const ownerResult = await db.addUser('Bob');
            if (!ownerResult.ok) throw new Error('Setup failed');
            const owner = ownerResult.value;

            // 2. Add map
            const addMapResult = await db.addMap('ForestLevel', owner, '/path/to/forest.map');
            expect(addMapResult.ok).toBe(true);
            if (addMapResult.ok) {
                expect(addMapResult.value.data.mapName).toBe('ForestLevel');
                expect(addMapResult.value.owner.id).toBe(owner.id);
            }

            // 3. List maps
            const listResult = await db.getMapList();
            expect(listResult.ok).toBe(true);
            if (listResult.ok) {
                expect(listResult.value).toHaveLength(1);
                expect(listResult.value[0].data.mapName).toBe('ForestLevel');
            }
        });

        it('should change map owner', async () => {
            const owner1 = (await db.addUser('Owner1')).value;
            const owner2 = (await db.addUser('Owner2')).value;
            const map = (await db.addMap('Level1', owner1, 'file1')).value;

            const changeResult = await db.changeOwner(map, owner2);
            expect(changeResult.ok).toBe(true);
            if (changeResult.ok) {
                expect(changeResult.value.owner.id).toBe(owner2.id);
                expect(changeResult.value.owner.data.nick).toBe('Owner2');
            }
        });

        it('should not allow duplicate map names', async () => {
            const owner = (await db.addUser('Charlie')).value;
            await db.addMap('SharedName', owner, 'file1');
            
            const result = await db.addMap('SharedName', owner, 'file2');
            expect(result.ok).toBe(false);
            if (!result.ok) {
                expect(result.error).toBe('Map name already taken');
            }
        });

        it('should delete a map', async () => {
            const owner = (await db.addUser('Dave')).value;
            await db.addMap('DeleteMe', owner, 'file1');
            
            let result = await db.deleteMap('DeleteMe');
            expect(result.ok).toBe(true);
            
            const getResult = await db.getMap('DeleteMe');
            expect(getResult.ok).toBe(false);
            
            // Delete non-existent map
            result = await db.deleteMap('DeleteMe');
            expect(result.ok).toBe(false);
        });
    });
});
