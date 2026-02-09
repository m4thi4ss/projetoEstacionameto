import { HttpInterceptorFn } from '@angular/common/http';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  
  console.log('🔑 Interceptor executado - Token:', token ? 'Presente' : 'Ausente');
  
  if (token) {
    const cloned = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    console.log('✅ Token adicionado ao header:', cloned.headers.get('Authorization')?.substring(0, 20) + '...');
    return next(cloned);
  }

  console.log('⚠️ Nenhum token disponível');
  return next(req);
};
