import Fastify from 'fastify';
import { setUpTheRoutes } from './src/set_up_the_routes.js';

const isProduction = process.env.NODE_ENV === 'production';
const fastify = Fastify({
    logger: isProduction ? { level: 'info' } : { level: 'debug' }
})

try {
    setUpTheRoutes(fastify)
    const port = process.env.PORT ? parseInt(process.env.PORT, 10) : 3000;
    const host = process.env.HOST || '0.0.0.0';
    await fastify.listen({ port, host })
}
catch (err) {
    fastify.log.error(err)
    process.exit(1)
}
