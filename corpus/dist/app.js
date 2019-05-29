"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var __asyncValues = (this && this.__asyncValues) || function (o) {
    if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
    var m = o[Symbol.asyncIterator], i;
    return m ? m.call(o) : (o = typeof __values === "function" ? __values(o) : o[Symbol.iterator](), i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () { return this; }, i);
    function verb(n) { i[n] = o[n] && function (v) { return new Promise(function (resolve, reject) { v = o[n](v), settle(resolve, reject, v.done, v.value); }); }; }
    function settle(resolve, reject, d, v) { Promise.resolve(v).then(function(v) { resolve({ value: v, done: d }); }, reject); }
};
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
var fs = __importStar(require("fs"));
var readline = __importStar(require("readline"));
var html2plaintext_1 = __importDefault(require("html2plaintext"));
var xregexp_1 = __importDefault(require("xregexp"));
var App = /** @class */ (function () {
    function App() {
        this.reg = xregexp_1.default('[^\\p{L}]', 'gim');
        this.url = xregexp_1.default('\\b(?:(?:https?|ftp|file):\\/\\/|www\\.|ftp\\.)[-A-Z0-9+&@#\\/%=~_|$?!:,.]*[A-Z0-9+&@#\\/%=~_|$]', 'img');
        this.email = xregexp_1.default('\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}\\b', 'img');
        this.phone = xregexp_1.default('^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\\s\\.\\/0-9]*$', 'img');
    }
    App.prototype.run = function (inputFile, outputFile) {
        return __awaiter(this, void 0, void 0, function () {
            var e_1, _a, e_2, _b, fileStream, lineReader, count, timer, lineReader_1, lineReader_1_1, line, e_1_1, writer, i, lineReader_2, lineReader_2_1, line, object, mainLabel, titleLabel, ad, title, mainLine, titleLine, e_2_1;
            return __generator(this, function (_c) {
                switch (_c.label) {
                    case 0:
                        fileStream = fs.createReadStream(inputFile);
                        lineReader = readline.createInterface({
                            input: fileStream,
                            crlfDelay: Infinity
                        });
                        count = 0;
                        console.log('Counting the lines...');
                        timer = setInterval(function () { return console.log(count); }, 1000);
                        _c.label = 1;
                    case 1:
                        _c.trys.push([1, 6, 7, 12]);
                        lineReader_1 = __asyncValues(lineReader);
                        _c.label = 2;
                    case 2: return [4 /*yield*/, lineReader_1.next()];
                    case 3:
                        if (!(lineReader_1_1 = _c.sent(), !lineReader_1_1.done)) return [3 /*break*/, 5];
                        line = lineReader_1_1.value;
                        count++;
                        _c.label = 4;
                    case 4: return [3 /*break*/, 2];
                    case 5: return [3 /*break*/, 12];
                    case 6:
                        e_1_1 = _c.sent();
                        e_1 = { error: e_1_1 };
                        return [3 /*break*/, 12];
                    case 7:
                        _c.trys.push([7, , 10, 11]);
                        if (!(lineReader_1_1 && !lineReader_1_1.done && (_a = lineReader_1.return))) return [3 /*break*/, 9];
                        return [4 /*yield*/, _a.call(lineReader_1)];
                    case 8:
                        _c.sent();
                        _c.label = 9;
                    case 9: return [3 /*break*/, 11];
                    case 10:
                        if (e_1) throw e_1.error;
                        return [7 /*endfinally*/];
                    case 11: return [7 /*endfinally*/];
                    case 12:
                        clearTimeout(timer);
                        fileStream.close();
                        console.log("Number of lines: " + count);
                        fileStream = fs.createReadStream(inputFile);
                        lineReader = readline.createInterface({
                            input: fileStream,
                            crlfDelay: Infinity
                        });
                        writer = fs.createWriteStream(outputFile, {
                            flags: 'a' // 'a' means appending (old data will be preserved)
                        });
                        i = 0;
                        timer = setInterval(function () { return console.log(((i / count) * 100).toFixed(2) + " (" + i + "/ " + count + ")"); }, 1000);
                        _c.label = 13;
                    case 13:
                        _c.trys.push([13, 18, 19, 24]);
                        lineReader_2 = __asyncValues(lineReader);
                        _c.label = 14;
                    case 14: return [4 /*yield*/, lineReader_2.next()];
                    case 15:
                        if (!(lineReader_2_1 = _c.sent(), !lineReader_2_1.done)) return [3 /*break*/, 17];
                        line = lineReader_2_1.value;
                        if (line != null) {
                            object = JSON.parse(line);
                            mainLabel = "__label__" + object['YRKE_ID'];
                            titleLabel = mainLabel;
                            ad = this.normalize(html2plaintext_1.default(object['PLATSBESKRIVNING']).replace('\r\n', ' ').replace('\n', ' ').trim());
                            title = this.normalize(html2plaintext_1.default(object['PLATSRUBRIK']).replace('\r\n', ' ').replace('\n', ' ').trim());
                            mainLine = "" + mainLabel + ad;
                            titleLine = "" + titleLabel + title;
                            i++;
                            if (ad.length > 30) {
                                writer.write(mainLine + "\n");
                            }
                            if (title.length > 10) {
                                writer.write(titleLine + "\n");
                            }
                        }
                        _c.label = 16;
                    case 16: return [3 /*break*/, 14];
                    case 17: return [3 /*break*/, 24];
                    case 18:
                        e_2_1 = _c.sent();
                        e_2 = { error: e_2_1 };
                        return [3 /*break*/, 24];
                    case 19:
                        _c.trys.push([19, , 22, 23]);
                        if (!(lineReader_2_1 && !lineReader_2_1.done && (_b = lineReader_2.return))) return [3 /*break*/, 21];
                        return [4 /*yield*/, _b.call(lineReader_2)];
                    case 20:
                        _c.sent();
                        _c.label = 21;
                    case 21: return [3 /*break*/, 23];
                    case 22:
                        if (e_2) throw e_2.error;
                        return [7 /*endfinally*/];
                    case 23: return [7 /*endfinally*/];
                    case 24:
                        clearTimeout(timer);
                        writer.close();
                        console.log(((i / count) * 100).toFixed(2) + " (" + i + "/ " + count + ")");
                        return [2 /*return*/];
                }
            });
        });
    };
    App.prototype.normalize = function (input) {
        var output = xregexp_1.default.replace(input, this.url, ' ', 'all');
        output = xregexp_1.default.replace(output, this.email, ' ', 'all');
        output = xregexp_1.default.replace(output, this.phone, ' ', 'all');
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
        output = xregexp_1.default.replace(output, this.reg, ' ', 'all').replace(/\s+/img, ' ').toLowerCase().trim();
        return output;
    };
    return App;
}());
exports.App = App;
//# sourceMappingURL=app.js.map