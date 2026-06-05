import Fastify from 'fastify';
import { set_up_the_routes } from './src/set_up_the_routes.js';

const isProduction = process.env.NODE_ENV === 'production';
const fastify = Fastify({
    logger: isProduction ? { level: 'info' } : { level: 'debug' }
})

try {
    set_up_the_routes(fastify)
    const port = process.env.PORT ? parseInt(process.env.PORT) : 3000;
    await fastify.listen({ port })
}
catch (err) {
    fastify.log.error(err)
    process.exit(1)
}
