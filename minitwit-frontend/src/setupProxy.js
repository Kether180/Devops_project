const {createProxyMiddleware} = require("http-proxy-middleware");

module.exports = function (app) {
  app.use(
    "/api",
    createProxyMiddleware({
      target: "http://157.230.79.99:5050",
      changeOrigin: true,
      pathRewrite: {
        "^/api": "/"
      }
    })
  );
};