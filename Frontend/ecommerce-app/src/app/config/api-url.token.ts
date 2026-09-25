import { InjectionToken } from '@angular/core';
import { environment } from '../../environments/environment';

declare global {
  interface Window {
    __ECOMMERCE_CONFIG__?: {
      apiUrl?: string;
    };
  }
}

export const API_URL = new InjectionToken<string>('API_URL', {
  factory: () => {
    const urlConfigurada = window.__ECOMMERCE_CONFIG__?.apiUrl?.trim();
    return (urlConfigurada || environment.apiUrl).replace(/\/$/, '');
  }
});
