import { getPostgresDatabaseConnection } from './postgres_connection.js';

async function seed() {
    const db = getPostgresDatabaseConnection();
    console.log("🌱 Starting Database Seed...");

    const users = ["Walrus", "AnonymousSloth"];

    try {
        for (const nick of users) {
            console.log(`Checking for user '${nick}'...`);
            const ownerRes = await db.getOwner(nick);

            if (ownerRes.ok) {
                console.log(`✅ User '${nick}' already exists (ID: ${ownerRes.value.id}).`);
            } else {
                console.log(`➕ Adding user '${nick}'...`);
                const addRes = await db.addUser(nick);
                if (addRes.ok) {
                    console.log(`✅ Successfully added user '${nick}' (ID: ${addRes.value.id}).`);
                } else {
                    console.error(`❌ Failed to add user '${nick}': ${addRes.error}`);
                }
            }
        }
    } catch (err: any) {
        console.error("❌ Critical seeding error:", err.message);
    } finally {
        console.log("🌲 Seeding process finished.");
        process.exit(0);
    }
}

seed();
