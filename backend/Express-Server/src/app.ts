import express, { Request, Response } from 'express';
import * as fs from 'fs';
import * as path from 'path';
import { promisify } from 'util';
import { exec } from 'child_process';
import { readdir, readFile } from 'fs/promises';
import { resolve } from 'path';
import bodyParser from 'body-parser';
import cors from 'cors';
import { runTs } from './execute-tests';

import swaggerUi from 'swagger-ui-express';

const swaggerDocument = require('../swagger.json');

const app = express();
const port = 3000;

app.use('/swagger', swaggerUi.serve, swaggerUi.setup(swaggerDocument));
app.use(cors());

app.use(bodyParser.json());

app.get('/', (req: Request, res: Response) => {
  res.send('Hello, Express!');
});

app.post('/api/execute/:exerciseId', async (req: Request, res: Response) => {
  const exerciseId = req.params.exerciseId;
  const code = req.body.code;
  console.log(code);
  console.log("11111111111111111111111111111111111111");
  const templateFilePath = `./templates/${exerciseId}`;
  const result = await runTs(exerciseId, templateFilePath,code);
  res.status(200).json(result);
});

app.listen(port, () => {
  console.log(`Server is running at http://localhost:${port}`);
});
