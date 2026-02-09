import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface Toast {
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastSubject = new Subject<Toast>();
  public toast$ = this.toastSubject.asObservable();

  success(message: string, duration: number = 3000) {
    this.show({ message, type: 'success', duration });
  }

  error(message: string, duration: number = 4000) {
    this.show({ message, type: 'error', duration });
  }

  warning(message: string, duration: number = 3500) {
    this.show({ message, type: 'warning', duration });
  }

  info(message: string, duration: number = 3000) {
    this.show({ message, type: 'info', duration });
  }

  private show(toast: Toast) {
    this.toastSubject.next(toast);
  }
}
