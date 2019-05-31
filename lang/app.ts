import * as fs from 'fs';
import * as readline from 'readline';
import franc from 'franc';
import h2p from 'html2plaintext';

export class App {

  private set = new Set<string>();

  private length: number = 20;

  async run(inputFile: string, outputFile: string, length: string) {

    this.length = parseFloat(length);

    let fileStream = fs.createReadStream(inputFile);

    let lineReader = readline.createInterface({

      input: fileStream,

      crlfDelay: Infinity
    });

    let count = 0;

    console.log('Counting the lines...');

    let timer = setInterval(() => console.log(count), 1000);

    for await (const line of lineReader) {

      count++;
    }

    clearTimeout(timer);

    fileStream.close();

    console.log(`Number of lines: ${count}`);

    fileStream = fs.createReadStream(inputFile);

    lineReader = readline.createInterface({

      input: fileStream,

      crlfDelay: Infinity
    });

    let i = 0;

    timer = setInterval(() => console.log(`${((i / count) * 100).toFixed(2)} (${i}/ ${count})`), 1000);

    const writer = fs.createWriteStream(outputFile);

    for await (const line of lineReader) {

      if (line != null) {

        const object = JSON.parse(line) as any;

        const id = object['YRKE_ID'];

        const ad = h2p(object['PLATSBESKRIVNING'] as string).replace('\r\n', ' ').replace('\n', ' ').trim();

        if (id != null && this.detect(ad)) {

          writer.write(`${id} ${ad}\n`);
        }

        i++;
      }
    }

    clearTimeout(timer);

    writer.close();

    console.log(`${((i / count) * 100).toFixed(2)} (${i}/ ${count})`);
  }

  detect(ad: string) {

    if (!this.set.has(ad)) {

      const language = franc(ad, { minLength: this.length, whitelist: ['swe', 'eng'] });

      this.set.add(ad);

      return language === 'swe';

    } else {

      return false;
    }
  }
}