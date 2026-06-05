import pg from 'pg';
const { Pool } = pg;
import type { DatabaseConnection, MapEntity, OwnerEntity, MapData, OwnerData, Result } from "./database_connection.js"

function getErrorMessage(err: any): string {
    if (err instanceof Error) {
        // Handle AggregateError specifically if it exists
        if ('errors' in err && Array.isArray((err as any).errors)) {
            return (err as any).errors.map((e: any) => e.message).join(', ');
        }
        return err.message;
    }
    if (typeof err === 'string') return err;
    return JSON.stringify(err);
}

class PostgresDatabase implements DatabaseConnection {
    private pool: pg.Pool;

    constructor() {
        this.pool = new Pool({
            // Uses standard PG environment variables:
            // PGUSER, PGHOST, PGDATABASE, PGPASSWORD, PGPORT
        });
    }

    private mapRowToOwner(row: any): OwnerEntity {
        return {
            id: row.id,
            data: { nick: row.nick }
        };
    }

    private mapRowToMapEntity(row: any): MapEntity {
        const owner: OwnerEntity = {
            id: row.ownerid,
            data: { nick: row.ownernick }
        };
        return {
            id: row.mapid,
            owner: owner,
            data: {
                mapName: row.mapname,
                owner: owner.data,
                filePath: row.filename
            }
        };
    }

    async getCurrentUser(): Promise<Result<OwnerEntity, string>> {
        /**
         * WARNING: Placeholder implementation. 
         * This currently returns the first user in the database without any authentication.
         * In a real application, this must be replaced with proper session/JWT verification.
         */
        try {
            const result = await this.pool.query('SELECT ID as id, Nick as nick FROM Owners LIMIT 1');
            if (result.rows.length > 0) {
                return { ok: true, value: this.mapRowToOwner(result.rows[0]) };
            }
            return { ok: false, error: "No users found" };
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }

    async getOwner(nick: string): Promise<Result<OwnerEntity, string>> {
        try {
            const result = await this.pool.query('SELECT ID as id, Nick as nick FROM Owners WHERE Nick = $1', [nick]);
            if (result.rows.length > 0) {
                return { ok: true, value: this.mapRowToOwner(result.rows[0]) };
            }
            return { ok: false, error: `User ${nick} not found` };
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }

    async addUser(nick: string): Promise<Result<OwnerEntity, string>> {
        try {
            const result = await this.pool.query(
                'INSERT INTO Owners (Nick) VALUES ($1) RETURNING ID as id, Nick as nick',
                [nick]
            );
            return { ok: true, value: this.mapRowToOwner(result.rows[0]) };
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }

    async getMap(mapName: string): Promise<Result<MapEntity, string>> {
        try {
            const result = await this.pool.query('SELECT * FROM MapsWithOwners WHERE MapName = $1', [mapName]);
            if (result.rows.length > 0) {
                return { ok: true, value: this.mapRowToMapEntity(result.rows[0]) };
            }
            return { ok: false, error: `Map ${mapName} not found` };
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }

    async getMapList(): Promise<Result<MapEntity[], string>> {
        try {
            const result = await this.pool.query('SELECT * FROM MapsWithOwners');
            return { ok: true, value: result.rows.map(row => this.mapRowToMapEntity(row)) };
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }

    async addMap(mapName: string, owner: OwnerEntity, filePath: string): Promise<Result<MapEntity, string>> {
        try {
            // Check if map already exists
            const existing = await this.getMap(mapName);
            if (existing.ok) {
                return { ok: false, error: "Map name already taken" };
            }

            await this.pool.query(
                'INSERT INTO MapData (MapName, FileName, OwnerID) VALUES ($1, $2, $3)',
                [mapName, filePath, owner.id]
            );

            return this.getMap(mapName);
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }

    async changeOwner(mapEntity: MapEntity, newOwner: OwnerEntity): Promise<Result<MapEntity, string>> {
        try {
            await this.pool.query(
                'UPDATE MapData SET OwnerID = $1 WHERE MapID = $2',
                [newOwner.id, mapEntity.id]
            );
            return this.getMap(mapEntity.data.mapName);
        } catch (err: any) {
            return { ok: false, error: getErrorMessage(err) };
        }
    }
}

let instance: DatabaseConnection | null = null;

export function getPostgresDatabaseConnection(): DatabaseConnection {
    if (!instance) {
        instance = new PostgresDatabase();
    }
    return instance;
}
