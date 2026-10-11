const http = require('http'), fs = require('fs'), path = require('path');
const root = require("path").resolve(__dirname, "..");
const types = { '.html': 'text/html; charset=utf-8', '.css': 'text/css', '.js': 'text/javascript', '.map': 'application/json' };
http.createServer((req, res) => {
  const p = path.normalize(path.join(root, decodeURIComponent(req.url.split('?')[0])));
  if (!p.startsWith(path.normalize(root))) { res.writeHead(403); return res.end(); }
  fs.readFile(p, (err, data) => {
    if (err) { res.writeHead(404); return res.end('not found'); }
    res.writeHead(200, { 'Content-Type': types[path.extname(p)] || 'application/octet-stream' });
    res.end(data);
  });
}).listen(5500, '127.0.0.1');
