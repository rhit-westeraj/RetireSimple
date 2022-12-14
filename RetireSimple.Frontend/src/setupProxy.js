const { createProxyMiddleware } = require('http-proxy-middleware');

const context = [
    "/api/Investment/GetAllInvestments",
    "/api/Investment/AddStock",
    "/api/Investment/AddRandomStock",
    "/api/Analysis/GetAnalysis",
];

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        target: 'https://localhost:7219/',
        secure: false,
    });

    app.use(appProxy);
};
