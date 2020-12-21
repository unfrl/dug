//Based on https://blog.woubuc.be/post/publishing-rust-binary-on-npm/
const { Binary } = require('binary-install');

function getBinary() {
    const version = require('../package.json').version;
    //Using curl+jq you can get the download link with something like this:
    // curl -sL https://git.kaijucode.com/api/v1/repos/matt/dug/releases?limit=1 | jq '.[0].assets[] | select(.name=="dug.0.0.25.linux-x64.deb") | .browser_download_url'
    // there is an npm package for jq so that should be doable that way, itll need to be aware of version as well as platform

    const url = "https://git.kaijucode.com/attachments/0977c9d2-4ce4-4571-8b9a-7aa8ff32774d"; //Currently this just gets one version and only for linux-x64, I can make it smarter and cross-platform
    return new Binary("dug", "https://git.kaijucode.com/attachments/0977c9d2-4ce4-4571-8b9a-7aa8ff32774d");
}

module.exports = getBinary;