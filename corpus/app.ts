import * as fs from 'fs';
import * as path from 'path';
import * as readline from 'readline';
import h2p from 'html2plaintext';
import XRegEx from 'xregexp';
import LanguageDetect from 'languagedetect';

export class App {

  private reg = XRegEx('[^\\p{L}]', 'gim');

  private url = XRegEx('\\b(?:(?:https?|ftp|file):\\/\\/|www\\.|ftp\\.)[-A-Z0-9+&@#\\/%=~_|$?!:,.]*[A-Z0-9+&@#\\/%=~_|$]', 'img');

  private email = XRegEx('\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}\\b', 'img');

  private phone = XRegEx('^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\\s\\.\\/0-9]*$', 'img');

  private writers = new Map<string, fs.WriteStream>();

  private set = new Set<string>();

  private languageDetector = new LanguageDetect();

  private outputDirectory: string = '.';

  private accurecy: number = 0.2;

  async run(inputFile: string, outputDirectory: string, accurecy:string) {

    this.accurecy = parseFloat(accurecy);

    this.outputDirectory = outputDirectory;

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

    for await (const line of lineReader) {

      if (line != null) {

        const object = JSON.parse(line) as any;

        const mainLabel = `__label__${object['YRKE_ID']}`;

        const ad = this.normalize(h2p(object['PLATSBESKRIVNING'] as string).replace('\r\n', ' ').replace('\n', ' ').trim());

        const adLine = `${mainLabel} ${ad}`;

        i++;

        this.write(adLine, ad);
      }
    }

    clearTimeout(timer);

    this.writers.forEach((writer) => writer.close());

    console.log(`${((i / count) * 100).toFixed(2)} (${i}/ ${count})`);
  }

  write(adLine: string, ad: string) {

    const languages = this.languageDetector.detect(ad, 1);

    if (languages.length > 0 && !this.set.has(ad)) {

      this.set.add(ad);

      let language = languages[0][0];

      const accurecy = languages[0][1];

      language = accurecy >= this.accurecy && (language === 'english' || language === 'swedish' || language === 'danish' || language === 'norwegian') ? language : 'other';

      let writer: fs.WriteStream;

      if (this.writers.has(language)) {

        writer = this.writers.get(language) as fs.WriteStream;
      }
      else {

        writer = fs.createWriteStream(path.join(this.outputDirectory, `${language}.corpus`));

        this.writers.set(language, writer);
      }

      writer.write(`${adLine}\n`);
    }
  }

  normalize(input: string) {

    let output = XRegEx.replace(input, this.url, ' ', 'all');

    output = XRegEx.replace(output, this.email, ' ', 'all');

    output = XRegEx.replace(output, this.phone, ' ', 'all');

    output = output

      .replace(/<.+?>/img, '')

      .replace(/0/img, ' noll ')

      .replace(/1/img, ' ett ')

      .replace(/2/img, ' två ')

      .replace(/3/img, ' tre ')

      .replace(/4/img, ' fyra ')

      .replace(/5/img, ' fem ')

      .replace(/6/img, ' sex ')

      .replace(/7/img, ' sju ')

      .replace(/8/img, ' åtta ')

      .replace(/9/img, ' nio ');

    output = XRegEx.replace(output, this.reg, ' ', 'all').replace(/\s+/img, ' ').toLowerCase().trim();

    return output;
  }
}