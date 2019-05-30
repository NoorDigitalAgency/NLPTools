import {App} from './app';

const app = new App();
const args = process.argv.slice(2);
app.run(args[0], args[1], args[2]);