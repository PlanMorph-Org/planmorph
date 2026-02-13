const fs = require('fs');
const path = require('path');

const filePath = process.argv[2];
if (!filePath) { console.error('Usage: node clip-svg.js <file>'); process.exit(1); }

let content = fs.readFileSync(filePath, 'utf8');

const clipDef = '<defs><clipPath id="round"><circle cx="480" cy="480" r="480"/></clipPath></defs><g clip-path="url(#round)">';

// Insert after opening <svg ...> tag
content = content.replace(
  /(xml:space="preserve">)/,
  '$1\n' + clipDef
);

// Wrap closing tag
content = content.replace(/<\/svg>\s*$/, '</g>\n</svg>\n');

fs.writeFileSync(filePath, content, 'utf8');
console.log('Done:', filePath);
