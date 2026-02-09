import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface DialogOptions {
  type: 'alert' | 'confirm';
  title: string;
  message: string;
  resolve?: (value: boolean) => void;
}

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  private dialogSubject = new BehaviorSubject<DialogOptions | null>(null);
  public dialog$ = this.dialogSubject.asObservable();

  /** Exibe um aviso com apenas o botão OK (estilo modal do app). */
  alert(title: string, message: string): void {
    this.dialogSubject.next({ type: 'alert', title, message });
  }

  /** Exibe confirmação com Confirmar e Cancelar. Retorna Promise<true> se Confirmar, Promise<false> se Cancelar. */
  confirm(title: string, message: string): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.dialogSubject.next({ type: 'confirm', title, message, resolve });
    });
  }

  /** Fecha o diálogo. Para confirm, passe true (Confirmar) ou false (Cancelar). */
  close(value?: boolean): void {
    const current = this.dialogSubject.value;
    if (current?.resolve !== undefined) {
      current.resolve(value === true);
    }
    this.dialogSubject.next(null);
  }
}
