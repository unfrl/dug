//Based on https://blog.woubuc.be/post/publishing-rust-binary-on-npm/
const { Binary } = require('binary-install');
const axios = require('axios').default;
const _ = require('lodash/core');

const os = require('os');

function getPlatformAssetPattern() {
    const type = os.type();
    const arch = os.arch();

    // if (type === 'Windows_NT' && arch === 'x64') return 'win64';
    // if (type === 'Windows_NT') return 'win32';
    if (type === 'Linux' && arch === 'x64') return 'linux-x64';
    // if (type === 'Darwin' && arch === 'x64') return 'macos';

    throw new Error(`Unsupported platform: ${type} ${arch}`);
}

async function getBinary() {
    const version = require('../package.json').version; //TODO: Use this instead of below!
    // const version = "0.0.25";

    //Using curl+jq you can get the download link with something like this:
    // curl -sL https://git.kaijucode.com/api/v1/repos/matt/dug/releases?limit=1 | jq '.[0].assets[] | select(.name=="dug.0.0.25.linux-x64.deb") | .browser_download_url'
    // there is an npm package for jq so that should be doable that way, itll need to be aware of version as well as platform
    
    var response = await axios.get("https://git.kaijucode.com/api/v1/repos/matt/dug/releases");
    var targetReleaseObject = _.filter(response.data, {tag_name: version})[0];
    var releaseAssets = targetReleaseObject.assets;
    // console.log("Available Assets: ", releaseAssets);
    var targetAsset = _.filter(releaseAssets, {name: `dug.${version}.${getPlatformAssetPattern()}.tar.gz`})[0];
    // console.log("Target Asset: ", targetAsset);
    var downloadURL = targetAsset.browser_download_url;
    // console.log("Target URL: ", downloadURL);
    return new Binary("dug", downloadURL);
}

module.exports = getBinary;