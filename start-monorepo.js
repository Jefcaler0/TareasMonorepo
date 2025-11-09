#!/usr/bin/env node
/**
 * Script multiplataforma para levantar el monorepo Tareas.
 * Ejecuta Docker Compose, inicia el backend (.NET) y el frontend (Vite).
 */

const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

const rootDir = __dirname;
const composeFile = path.join(rootDir, 'docker', 'docker-compose.yml');
const backendDir = path.join(rootDir, 'backend', 'Tareas.API');
const frontendDir = path.join(rootDir, 'frontend');

const runningProcesses = [];
let isShuttingDown = false;

function loadEnvFile() {
  const envPath = path.join(rootDir, '.env');
  if (!fs.existsSync(envPath)) {
    console.log('No se encontró un archivo .env en la raíz (se continuara con las variables actuales).');
    return;
  }

  const lines = fs.readFileSync(envPath, 'utf8').split(/\r?\n/);
  for (const line of lines) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('#')) continue;
    const eqIndex = trimmed.indexOf('=');
    if (eqIndex === -1) continue;
    const key = trimmed.slice(0, eqIndex).trim();
    let value = trimmed.slice(eqIndex + 1).trim();
    if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) {
      value = value.slice(1, -1);
    }
    if (!(key in process.env)) {
      process.env[key] = value;
    }
  }

  console.log('Variables de entorno cargadas desde .env.');
}

function runOnce(name, command, cwd = rootDir) {
  return new Promise((resolve, reject) => {
    console.log(`[${name}] ${command}`);
    const child = spawn(command, {
      cwd,
      stdio: 'inherit',
      shell: true,
      env: process.env
    });

    child.on('exit', code => {
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`[${name}] finalizó con código ${code}.`));
      }
    });

    child.on('error', err => reject(new Error(`[${name}] no se pudo iniciar: ${err.message}`)));
  });
}

function startPersistent(name, command, cwd) {
  console.log(`[${name}] ${command}`);
  const child = spawn(command, {
    cwd,
    stdio: 'inherit',
    shell: true,
    env: process.env
  });

  child.on('exit', code => {
    console.log(`[${name}] finalizó${code === null ? '' : ` con código ${code}`}.`);
    shutdown(code ?? 0);
  });

  child.on('error', err => {
    console.error(`[${name}] error: ${err.message}`);
    shutdown(1);
  });

  runningProcesses.push({ name, child });
  return child;
}

async function shutdown(exitCode = 0) {
  if (isShuttingDown) return;
  isShuttingDown = true;

  console.log('\nIniciando apagado limpio...');

  for (const { name, child } of runningProcesses) {
    if (!child.killed) {
      console.log(`Cerrando ${name}...`);
      child.kill('SIGINT');
    }
  }

  await Promise.all(
    runningProcesses.map(
      ({ child }) =>
        new Promise(resolve => {
          child.on('close', resolve);
          setTimeout(resolve, 3000);
        })
    )
  );

  try {
    await runOnce('docker:down', `docker compose -f "${composeFile}" down`);
  } catch (err) {
    console.warn(`No se pudo detener Docker Compose: ${err.message}`);
  }

  process.exit(exitCode);
}

async function main() {
  loadEnvFile();

  if (!process.env.ASPNETCORE_ENVIRONMENT) {
    process.env.ASPNETCORE_ENVIRONMENT = 'Development';
  }

  if (!process.env.ASPNETCORE_URLS) {
    process.env.ASPNETCORE_URLS = 'http://localhost:5062';
  }

  await runOnce('docker:up', `docker compose -f "${composeFile}" up -d`);

  if (!fs.existsSync(path.join(backendDir, 'bin'))) {
    await runOnce('dotnet:restore', 'dotnet restore', backendDir);
  }

  if (!fs.existsSync(path.join(frontendDir, 'node_modules'))) {
    await runOnce('pnpm:install', 'pnpm install', frontendDir);
  }

  startPersistent('backend', `dotnet run --urls "${process.env.ASPNETCORE_URLS}"`, backendDir);

  const viteCommand = process.env.VITE_HOST
    ? `pnpm dev -- --host ${process.env.VITE_HOST}`
    : 'pnpm dev';
  startPersistent('frontend', viteCommand, frontendDir);

  console.log('\nServicios ejecutándose:');
  console.log(`- API:       ${process.env.ASPNETCORE_URLS}`);
  console.log(`- Swagger:   ${process.env.ASPNETCORE_URLS}/swagger`);
  console.log(`- Frontend:  http://localhost:5173`);
  console.log('\nPresiona Ctrl+C para detener todo.');
}

process.on('SIGINT', () => shutdown(0));
process.on('SIGTERM', () => shutdown(0));
process.on('uncaughtException', err => {
  console.error('Error no controlado:', err);
  shutdown(1);
});
process.on('unhandledRejection', reason => {
  console.error('Promesa rechazada sin manejar:', reason);
  shutdown(1);
});

main().catch(err => {
  console.error(err.message);
  shutdown(1);
});


