import type { Config } from 'jest';

const config: Config = {
    preset: 'ts-jest/presets/default-esm',
    testEnvironment: 'node',
    testRegex: '(/__tests__/.*)\\.[mc]?[jt]sx?$',
    testPathIgnorePatterns: ['/__integration_tests__/'],
    transform: {
        "^.+\\.ts$": ["ts-jest", {
            tsconfig: "./tsconfig.test.json",
            useESM: true,
        }]
    },
    moduleNameMapper: {
        '^(\\.{1,2}/.*)\\.js$': '$1',
    },
    moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json', 'node'],
};

export default config;
