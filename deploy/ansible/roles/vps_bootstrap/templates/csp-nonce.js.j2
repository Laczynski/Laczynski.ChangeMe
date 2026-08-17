function issueNonce(r) {
  var cached = r.variables.csp_nonce_cache;
  if (cached) {
    return cached;
  }

  var crypto = require('crypto');
  var seed = r.variables.request_id + String(Date.now()) + String(Math.random());
  var nonce = crypto.createHash('sha256').update(seed).digest('base64').slice(0, 22);

  r.variables.csp_nonce_cache = nonce;
  return nonce;
}

export default { issueNonce };
