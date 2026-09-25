import { mkdir, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';

const apiUrl = process.env.ECOMMERCE_API_URL?.trim();
if (!apiUrl) {
  throw new Error('Falta la variable ECOMMERCE_API_URL para generar la configuración runtime.');
}

let parsedUrl;
try {
  parsedUrl = new URL(apiUrl);
} catch {
  throw new Error('ECOMMERCE_API_URL debe ser una URL absoluta válida.');
}

if (!['http:', 'https:'].includes(parsedUrl.protocol)) {
  throw new Error('ECOMMERCE_API_URL debe utilizar http o https.');
}

const outputDirectory = resolve('dist/ecommerce-app/browser');
const outputFile = resolve(outputDirectory, 'app-config.js');
const normalizedUrl = apiUrl.replace(/\/$/, '');
const contents = `window.__ECOMMERCE_CONFIG__ = ${JSON.stringify({ apiUrl: normalizedUrl })};\n`;

await mkdir(outputDirectory, { recursive: true });
await writeFile(outputFile, contents, 'utf8');
console.log(`Configuración runtime generada para ${normalizedUrl}`);
