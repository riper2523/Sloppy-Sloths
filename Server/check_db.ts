import pg from 'pg';
const { Client } = pg;

async function check() {
    const client = new Client();
    await client.connect();
    const res = await client.query('SELECT * FROM Owners');
    console.table(res.rows);
    await client.end();
}

check().catch(console.error);
