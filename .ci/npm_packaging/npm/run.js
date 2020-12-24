#!/usr/bin/env node

const getBinary = require('./getBinary');
getBinary()
    .then(res => res.run());