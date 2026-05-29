import { Injectable } from '@angular/core';
import { AppConfig } from './app-config';
import { environment } from '../../../environments/environment';
import { APP_NAME, API_BASE_URL } from '../constants';

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private _config: AppConfig;

  constructor() {
    this._config = {
      apiUrl: environment.apiUrl || API_BASE_URL,
      appName: APP_NAME,
      version: '0.1.0',
    };
  }

  get config(): AppConfig {
    return this._config;
  }

  get apiUrl(): string {
    return this._config.apiUrl;
  }

  get appName(): string {
    return this._config.appName;
  }
}