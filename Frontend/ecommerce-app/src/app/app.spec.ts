import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideZonelessChangeDetection()]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should keep admin routes behind authorization guards', () => {
    const admin = routes.find(route => route.path === 'admin');
    const checkout = routes.find(route => route.path === 'checkout');

    expect(admin?.canActivate?.length).toBeGreaterThan(0);
    expect(checkout?.canActivate?.length).toBeGreaterThan(0);
  });
});
