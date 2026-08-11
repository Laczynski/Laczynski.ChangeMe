import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AppShellComponent } from '@core/layout/components/app-shell/app-shell.component';
import { ConfirmationService, MessageService } from 'primeng/api';
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
      providers: [MessageService, ConfirmationService]
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
