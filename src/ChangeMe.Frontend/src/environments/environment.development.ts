import packageJson from '../../package.json';

export const environment = {
  production: false,
  appVersion: packageJson.version,
  apiUrl: 'http://localhost:5000/api/v1'
};
