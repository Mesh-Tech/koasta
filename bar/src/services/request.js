export default class Request {
  static post(url) {
    return new this('post', url)
  }

  static get(url) {
    return new this('get', url)
  }

  static put(url) {
    return new this('put', url)
  }

  static patch(url) {
    return new this('patch', url)
  }

  static delete(url) {
    return new this('delete', url)
  }

  constructor(method, url) {
    this._method = method
    this._url = url
    this.header = {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    }
    this._blobs = []
  }

  withCredentials() {
    this._credentials = true
    return this
  }

  send(obj) {
    this._send = obj
    return this
  }

  set(k, v) {
    this.header[k] = v
    return this
  }

  _build() {
    var res
    return fetch(this._url, {
      method: this._method.toUpperCase(),
      credentials: 'include',
      mode: 'cors',
      headers: this.header,
      body: this._send ? JSON.stringify(this._send) : undefined,
    })
      .then((r) => {
        res = r
        return r.text()
      })
      .then((body) => {
        return {
          status: res.status,
          body: body ? JSON.parse(body) : undefined,
        }
      })
  }

  then(resolve, reject) {
    return this._build().then(resolve, reject)
  }

  catch(reject) {
    return this._build().catch(reject)
  }
}

export const post = (url) => Request.post(url)
export const get = (url) => Request.get(url)
export const put = (url) => Request.put(url)
export const patch = (url) => Request.patch(url)
