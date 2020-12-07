export default class ConfigService {
  get apiUrl() {
    return process.env.API_URL || 'https://api.koasta.com'
  }

  get apiKey() {
    return process.env.API_KEY || ''
  }

  get queueUrl() {
    return process.env.QUEUE_URL || 'http://127.0.0.1:5002/event/venue'
  }

  get queueUsername() {
    return process.env.QUEUE_USERNAME || 'root'
  }

  get queuePassword() {
    return process.env.QUEUE_PASSWORD || 'password'
  }
}
