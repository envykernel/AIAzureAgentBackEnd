# 🔧 Configuration CORS - MultiAgentsBeta API

## 📋 Problème résolu
Votre frontend ReactJS sur `http://localhost:5174` ne pouvait pas appeler l'API à cause des restrictions CORS.

## ✅ Solution implémentée

### 1. **Configuration CORS dans Program.cs**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174", "https://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### 2. **Origines autorisées**
- `http://localhost:5174` (Vite/React par défaut)
- `https://localhost:5174`
- `http://localhost:3000` (Create React App)
- `https://localhost:3000`
- `http://localhost:4200` (Angular)
- `https://localhost:4200`

### 3. **Configuration flexible**
Les origines sont configurables via `appsettings.Development.json`

## 🧪 Test de la configuration

### **Endpoint de test CORS**
```http
GET {{baseUrl}}/api/test/cors
```

### **Endpoint de santé**
```http
GET {{baseUrl}}/api/test/health
```

## 🚀 Redémarrage requis

**IMPORTANT** : Après ces modifications, vous devez :

1. **Arrêter** votre API
2. **Redémarrer** votre API
3. **Tester** depuis votre frontend

## 🔍 Vérification

### **Depuis le navigateur (Console)**
```javascript
// Test CORS
fetch('http://localhost:5107/api/test/cors')
  .then(response => response.json())
  .then(data => console.log('CORS OK:', data))
  .catch(error => console.error('CORS Error:', error));

// Test santé
fetch('http://localhost:5107/api/test/health')
  .then(response => response.json())
  .then(data => console.log('Health OK:', data))
  .catch(error => console.error('Health Error:', error));
```

### **Depuis Postman**
- Importez la collection mise à jour
- Testez les endpoints de test
- Vérifiez que les requêtes passent

## 📱 Frontend ReactJS

### **Configuration axios/fetch**
```javascript
const API_BASE_URL = 'http://localhost:5107';

// Test CORS
const testCors = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/test/cors`);
    const data = await response.json();
    console.log('CORS working:', data);
  } catch (error) {
    console.error('CORS error:', error);
  }
};
```

## 🚨 Dépannage

### **Si CORS ne fonctionne toujours pas :**

1. **Vérifiez que l'API a redémarré**
2. **Vérifiez les ports** dans la configuration
3. **Vérifiez l'ordre des middlewares** dans Program.cs
4. **Vérifiez les logs** de l'API

### **Ordre correct des middlewares :**
```csharp
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");  // ← Doit être avant MapControllers
app.MapControllers();
```

## ✅ Résultat attendu

Après redémarrage, votre frontend ReactJS sur `http://localhost:5174` pourra :
- ✅ Appeler tous les endpoints de l'API
- ✅ Envoyer des requêtes POST/PUT/DELETE
- ✅ Recevoir des réponses JSON
- ✅ Gérer les sessions de chat sans problème CORS

---

**🎉 Votre API est maintenant prête pour le frontend !**

