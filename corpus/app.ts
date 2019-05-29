import * as fs from 'fs';
import * as readline from 'readline';
import h2p from 'html2plaintext';
import XRegEx from 'xregexp';

export class App {
  constructor() {
  }

  private reg = XRegEx('[^\\p{L}]', 'gim');

  private url = XRegEx('\\b(?:(?:https?|ftp|file):\\/\\/|www\\.|ftp\\.)[-A-Z0-9+&@#\\/%=~_|$?!:,.]*[A-Z0-9+&@#\\/%=~_|$]', 'img');

  private email = XRegEx('\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}\\b', 'img');

  private phone = XRegEx('^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\\s\\.\\/0-9]*$', 'img');

  async run(inputFile: string, outputFile: string) {

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

    const writer = fs.createWriteStream(outputFile, {
      flags: 'a' // 'a' means appending (old data will be preserved)
    });

    let i = 0;

    timer = setInterval(() => console.log(`${((i / count) * 100).toFixed(2)} (${i}/ ${count})`), 1000);

    for await (const line of lineReader) {
      if (line != null) {
        const object = JSON.parse(line) as any;
        const mainLabel = `__label__${object['YRKE_ID']}`;
        const titleLabel = mainLabel;
        const ad = this.normalize(h2p(object['PLATSBESKRIVNING'] as string).replace('\r\n', ' ').replace('\n', ' ').trim());
        const title = this.normalize(h2p(object['PLATSRUBRIK'] as string).replace('\r\n', ' ').replace('\n', ' ').trim());
        const mainLine = `${mainLabel}${ad}`;
        const titleLine = `${titleLabel}${title}`;
        i++;
        if (ad.length > 30) {
          writer.write(`${mainLine}\n`);
        }
        if (title.length > 10) {
          writer.write(`${titleLine}\n`);
        }
      }
    }

    clearTimeout(timer);

    writer.close();

    console.log(`${((i / count) * 100).toFixed(2)} (${i}/ ${count})`);
  }

  normalize(input:string)
  {
    let output =  XRegEx.replace(input, this.url, ' ', 'all');

    output =  XRegEx.replace(output, this.email, ' ', 'all');

    output =  XRegEx.replace(output, this.phone, ' ', 'all');

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