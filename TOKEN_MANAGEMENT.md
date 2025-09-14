# 🎯 Gestion des Tokens et Messages - MultiAgentsBeta API

## 📊 Nouvelles fonctionnalités ajoutées

Votre API retourne maintenant des informations complètes sur la gestion des tokens et des messages dans chaque réponse de chat.

---

## 🔍 Propriétés ajoutées dans ChatResponse

### **📈 Statistiques des messages**
- `totalMessageCount` : Nombre total de messages dans la session
- `totalTokenCount` : Nombre total de tokens utilisés dans la session

### **🎯 Gestion des tokens**
- `maxTokens` : Limite maximale de tokens par session (défaut: 4000)
- `remainingTokens` : Tokens restants avant d'atteindre la limite
- `tokenUsagePercentage` : Pourcentage d'utilisation des tokens (0.0 à 100.0)

---

## 📋 Exemple de réponse complète

### **🆕 Nouvelle session**
```json
{
  "sessionId": "session_789012",
  "message": "Hello! I'm your AI assistant. How can I help you today?",
  "role": "assistant",
  "timestamp": "2024-01-15T10:30:00Z",
  "tokenCount": 45,
  "isNewSession": true,
  "totalMessageCount": 2,
  "totalTokenCount": 45,
  "maxTokens": 4000,
  "remainingTokens": 3955,
  "tokenUsagePercentage": 1.125
}
```

### **🔄 Session existante**
```json
{
  "sessionId": "session_123456",
  "message": "I understand you want to continue our conversation...",
  "role": "assistant",
  "timestamp": "2024-01-15T10:35:00Z",
  "tokenCount": 38,
  "isNewSession": false,
  "totalMessageCount": 8,
  "totalTokenCount": 1250,
  "maxTokens": 4000,
  "remainingTokens": 2750,
  "tokenUsagePercentage": 31.25
}
```

---

## 🧮 Calculs automatiques

### **📊 Tokens restants**
```
remainingTokens = maxTokens - totalTokenCount
```

### **📈 Pourcentage d'utilisation**
```
tokenUsagePercentage = (totalTokenCount / maxTokens) × 100
```

### **⚠️ Résumé automatique**
Quand `tokenUsagePercentage > 80%`, le système :
1. Génère automatiquement un résumé de la conversation
2. Réduit le `totalTokenCount`
3. Augmente le `remainingTokens`
4. Maintient la cohérence du contexte

---

## 🎨 Cas d'usage frontend

### **📱 Affichage des statistiques**
```javascript
const ChatResponse = ({ response }) => {
  return (
    <div className="chat-response">
      <div className="message">{response.message}</div>
      
      {/* Statistiques de la session */}
      <div className="session-stats">
        <div className="token-info">
          <span>Tokens utilisés: {response.totalTokenCount}/{response.maxTokens}</span>
          <span>Reste: {response.remainingTokens}</span>
          <span>Usage: {response.tokenUsagePercentage.toFixed(1)}%</span>
        </div>
        
        <div className="message-info">
          <span>Messages: {response.totalMessageCount}</span>
        </div>
      </div>
      
      {/* Barre de progression des tokens */}
      <div className="token-progress">
        <div 
          className="progress-bar" 
          style={{ width: `${response.tokenUsagePercentage}%` }}
        />
      </div>
    </div>
  );
};
```

### **🚨 Alertes de limite**
```javascript
const checkTokenLimit = (response) => {
  if (response.tokenUsagePercentage > 70) {
    return {
      level: 'warning',
      message: `Attention: ${response.remainingTokens} tokens restants`
    };
  }
  
  if (response.tokenUsagePercentage > 90) {
    return {
      level: 'danger',
      message: 'Limite de tokens presque atteinte !'
    };
  }
  
  return { level: 'info', message: 'Utilisation normale des tokens' };
};
```

---

## 🔄 Workflow complet

### **1. Création de session**
```
POST /api/agent/chat
{
  "sessionId": "",
  "message": "Bonjour"
}
↓
Réponse avec isNewSession: true
totalMessageCount: 2 (utilisateur + assistant)
totalTokenCount: 45
remainingTokens: 3955
```

### **2. Conversation continue**
```
POST /api/agent/chat
{
  "sessionId": "session_123",
  "message": "Continuez"
}
↓
Réponse avec isNewSession: false
totalMessageCount: 4
totalTokenCount: 125
remainingTokens: 3875
```

### **3. Résumé automatique (si >80%)**
```
Quand tokenUsagePercentage > 80%
↓
Système génère un résumé
↓
totalTokenCount réduit
remainingTokens augmenté
Contexte maintenu
```

---

## ✅ Avantages de cette approche

1. **📊 Transparence totale** sur l'utilisation des ressources
2. **🎯 Gestion proactive** des limites de tokens
3. **🔄 Résumé automatique** pour optimiser l'utilisation
4. **📱 Interface utilisateur** riche en informations
5. **⚡ Performance optimisée** avec gestion intelligente

---

## 🚀 Utilisation recommandée

### **Frontend ReactJS**
- Afficher les statistiques en temps réel
- Créer des barres de progression visuelles
- Alerter l'utilisateur des limites approchantes
- Permettre la gestion manuelle des sessions

### **Monitoring**
- Suivre l'utilisation des tokens par session
- Analyser les patterns de conversation
- Optimiser les limites de tokens
- Détecter les sessions problématiques

**🎉 Votre API fournit maintenant une gestion complète et transparente des tokens et messages !**

