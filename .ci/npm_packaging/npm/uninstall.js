function getBinary() {
    try {
        const getBinary = require('./getBinary');
        return getBinary();
    } catch (err) { }
}

const binary = getBinary();
if (binary) {
    binary.uninstall();
}