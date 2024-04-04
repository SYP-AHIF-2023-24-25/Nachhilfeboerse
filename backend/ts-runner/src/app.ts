import express, { Request, Response } from 'express';
import bodyParser from 'body-parser';
import cors from 'cors';
import { runTs } from './execute-tests';
import fs from 'fs';
import swaggerUi from 'swagger-ui-express';
import { Snippets } from './model/snippets';
import path from 'path';
import { promisify } from 'util';
import { exec } from 'child_process';
import multer from 'multer';
import unzipper from 'unzipper'; // Add unzipper import
import { runTemplate } from './execute-tests';

const swaggerDocument = require('../swagger.json');

const app = express();
const port = 3000;

app.use('/swagger', swaggerUi.serve, swaggerUi.setup(swaggerDocument));
app.use(cors());
app.use(bodyParser.json());

app.get('/', (req: Request, res: Response) => {
  res.send('Hello, Express!');
});

app.post('/api/execute/:exerciseName', async (req: Request, res: Response) => {
  const exerciseName = req.params.exerciseName;
  const fileName = req.body.fileName;
  const code = req.body.code;
  const templateFilePath = `./templates/${exerciseName}`;
  console.log(fileName);
  console.log(code);
  const result = await runTs(exerciseName, templateFilePath, code, fileName);
  res.status(200).json(result);
});

const storage = multer.diskStorage({
  destination: function (req: any, file: any, cb: any) {
    cb(null, './templates'); // Save uploaded files to the "uploads" directory
  },
  filename: function (req: any, file: any, cb: any) {
    cb(null, file.originalname);
  }
});

const upload = multer({ storage: storage });

// Upload route with zip file extraction
app.post('/uploadFullTemplate', upload.single('file'), async (req, res) => {
  // Check if a file was uploaded

  if (!req.file) {
    return res.status(400).json({ error: 'No file uploaded' });
  }
  // Extract the uploaded zip file
  const zipFilePath = path.join(__dirname, '../templates', req.file.filename);
  await unZip(zipFilePath);
  console.log("after unzipping ...");
    const fileNameSplitted: string [] = req.file.filename.split('.');
    console.log(fileNameSplitted[0]);
    let result = "...";
    console.log(req.body.content);
    if(req.body.content === "full") {

      result = await runTemplate(`templates/${fileNameSplitted[0]}`);
      console.log(result);
    }
    else{
      result = "empty template uploaded";
    }
    console.log("before the end ...");

    // Remove the unzipped directory
    if(req.body.content === "full") {
      const unzippedDirPath = path.join(__dirname, '../templates', fileNameSplitted[0]);
      await deleteFolderRecursive(unzippedDirPath);
    }

    fs.unlinkSync(zipFilePath);
    res.status(200).json(result);
});

async function deleteFolderRecursive(path:string) {
  if (fs.existsSync(path)) {
    fs.readdirSync(path).forEach((file, index) => {
      const curPath = path + "/" + file;
      if (fs.lstatSync(curPath).isDirectory()) { // recurse
        deleteFolderRecursive(curPath);
      } else { // delete file
        fs.unlinkSync(curPath);
      }
    });
    fs.rmdirSync(path);
  }
}

async function unZip(zipFilePath: string): Promise<void> {
  return new Promise<void>((resolve, reject) => {
    fs.createReadStream(zipFilePath)
      .pipe(unzipper.Extract({path: `/usr/src/app/templates`}))
      .on('close', () => {
        console.log('Zip file extracted successfully');
        resolve();
      })
      .on('error', (err) => {
        console.error('Error extracting zip file:', err);
        reject(err);
      });
  });
}

app.listen(port, () => {
  console.log(`Server is running at http://localhost:${port}`);
});
