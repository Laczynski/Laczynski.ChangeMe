import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AppShellComponent } from '@core/layout/components/app-shell/app-shell.component';
import { LayoutService } from '@core/layout/services/layout.service';
import { AppComponent } from './app.component';

@Component({
  selector: 'app-shell',
  template: ''
})
class AppShellStubComponent {}

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        {
          provide: LayoutService,
          useValue: {
            $themeMode: signal('light')
          }
        }
      ]
    })
      .overrideComponent(AppComponent, {
        remove: { imports: [AppShellComponent] },
        add: { imports: [AppShellStubComponent] }
      })
      .compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render app shell', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-shell')).toBeTruthy();
  });
});
