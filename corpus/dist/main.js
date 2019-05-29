"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const app_1 = require("./app");
const app = new app_1.App();
const args = process.argv.slice(2);
app.run(args[0], args[1]);
//# sourceMappingURL=main.js.map