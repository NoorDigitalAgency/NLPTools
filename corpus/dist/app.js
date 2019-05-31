"use strict";
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (Object.hasOwnProperty.call(mod, k)) result[k] = mod[k];
    result["default"] = mod;
    return result;
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fs = __importStar(require("fs"));
const path = __importStar(require("path"));
const readline = __importStar(require("readline"));
const franc_1 = __importDefault(require("franc"));
const html2plaintext_1 = __importDefault(require("html2plaintext"));
const xregexp_1 = __importDefault(require("xregexp"));
class App {
    constructor() {
        this.reg = xregexp_1.default('[^\\p{L} ]', 'gim');
        this.url = xregexp_1.default('\\b(?:(?:https?|ftp|file):\\/\\/|www\\.|ftp\\.)[-A-Z0-9+&@#\\/%=~_|$?!:,.]*[A-Z0-9+&@#\\/%=~_|$]', 'img');
        this.email = xregexp_1.default('\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}\\b', 'img');
        this.phone = xregexp_1.default('^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\\s\\.\\/0-9]*$', 'img');
        this.writers = new Map();
        this.set = new Set();
        this.outputDirectory = '.';
        this.length = 20;
    }
    async run(inputFile, outputDirectory, length) {
        this.length = parseFloat(length);
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
                const object = JSON.parse(line);
                const mainLabel = ''; //`__label__${object['YRKE_ID']}`;
                const ad = this.normalize(html2plaintext_1.default(object['PLATSBESKRIVNING']).replace('\r\n', ' ').replace('\n', ' ').trim());
                const adLine = `${mainLabel} ${ad}`;
                i++;
                this.write(adLine, ad);
            }
        }
        clearTimeout(timer);
        this.writers.forEach((writer) => writer.close());
        console.log(`${((i / count) * 100).toFixed(2)} (${i}/ ${count})`);
    }
    write(adLine, ad) {
        if (!this.set.has(ad)) {
            const language = franc_1.default(ad, { minLength: this.length, whitelist: ['swe', 'eng', 'nor', 'dan', 'fin'] });
            this.set.add(ad);
            let writer;
            if (this.writers.has(language)) {
                writer = this.writers.get(language);
            }
            else {
                writer = fs.createWriteStream(path.join(this.outputDirectory, `${language}.corpus`));
                this.writers.set(language, writer);
            }
            writer.write(`${adLine}\n`);
        }
    }
    normalize(input) {
        let output = xregexp_1.default.replace(input, this.url, ' ', 'all');
        output = xregexp_1.default.replace(output, this.email, ' ', 'all');
        output = xregexp_1.default.replace(output, this.phone, ' ', 'all');
        output = output.replace(/NULL/mg, ' ').replace(/<\/s>/img, ' ').replace(/<.+?>/img, ' ').replace(/[0-9]/img, ' ');
        output = xregexp_1.default.replace(output, this.reg, ' ', 'all').replace(/\s+/img, ' ').toLowerCase().trim();
        return output;
    }
}
exports.App = App;
//# sourceMappingURL=app.js.map