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
const readline = __importStar(require("readline"));
const franc_1 = __importDefault(require("franc"));
const html2plaintext_1 = __importDefault(require("html2plaintext"));
class App {
    constructor() {
        this.set = new Set();
        this.length = 20;
    }
    async run(inputFile, outputFile, length) {
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
                const object = JSON.parse(line);
                const id = object['YRKE_ID'];
                const ad = html2plaintext_1.default(object['PLATSBESKRIVNING']).replace('\r\n', ' ').replace('\n', ' ').trim();
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
    detect(ad) {
        if (!this.set.has(ad)) {
            const language = franc_1.default(ad, { minLength: this.length, whitelist: ['swe', 'eng'] });
            this.set.add(ad);
            return language === 'swe';
        }
        else {
            return false;
        }
    }
}
exports.App = App;
//# sourceMappingURL=app.js.map