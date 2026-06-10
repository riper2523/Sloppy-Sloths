import pg from 'pg';
const { Client } = pg;
import { readFile, rm } from 'node:fs/promises';
import { join } from 'node:path';

async function reset() {
    const client = new Client();
    const dbName = process.env.PGDATABASE ?? '';
    if (!process.env.FORCE_DB_RESET && !dbName.endsWith('_test')) {
        throw new Error(`Refusing to reset database '${dbName}'. Use a '*_test' database or set FORCE_DB_RESET=1 to override.`);
    }

    const MAP_FILES_DIR = process.env.MAP_STORAGE_PATH
        ? join(process.cwd(), process.env.MAP_STORAGE_PATH)
        : join(process.cwd(), '..', 'App-Data', 'map-files');

    console.log("🔄 Resetting Database and Files...");

    try {
        await client.connect();

        // 1. Drop existing structures (order matters for foreign keys)
        console.log("🗑️ Dropping existing tables and views...");
        await client.query('DROP VIEW IF EXISTS MapsWithOwners CASCADE');
        await client.query('DROP TABLE IF EXISTS MapData CASCADE');
        await client.query('DROP TABLE IF EXISTS Owners CASCADE');

        // 2. Delete physical map files
        console.log(`📂 Clearing map files at ${MAP_FILES_DIR}...`);
        await rm(MAP_FILES_DIR, { recursive: true, force: true });

        // 3. Read init.sql
        console.log("📜 Reading init.sql...");
        const sqlPath = join(process.cwd(), 'database', 'schema', 'init.sql');
        const sql = await readFile(sqlPath, 'utf-8');

        // 4. Execute init.sql
        console.log("🏗️ Re-initializing schema...");
        await client.query(sql);

        console.log("✅ Server successfully reset (DB cleared and files deleted).");

    } catch (err: any) {
        console.error("❌ Reset failed:", err.message);
    } finally {
        await client.end();
        process.exit(0);
    }
}

reset();
