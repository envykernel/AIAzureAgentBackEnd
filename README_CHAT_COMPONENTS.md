# 💬 Composants de Chat avec Compteur de Caractères

## 🎯 Vue d'ensemble

Ce projet contient des composants ReactJS modernes pour créer une interface de chat avec un **compteur de caractères en temps réel** et une gestion complète des sessions MultiAgentsBeta.

---

## 📁 Structure des fichiers

```
├── ChatInputWithCounter.jsx    # Composant principal avec compteur
├── ChatInputWithCounter.css    # Styles du composant input
├── ChatExample.jsx             # Exemple d'utilisation complet
├── ChatExample.css             # Styles de l'exemple
└── README_CHAT_COMPONENTS.md   # Ce fichier
```

---

## 🚀 Composant principal : ChatInputWithCounter

### **✨ Fonctionnalités**

- **📊 Compteur de caractères** en temps réel
- **🎨 Changement de couleur** selon l'utilisation (Vert → Orange → Rouge)
- **📏 Auto-resize** du textarea
- **⌨️ Raccourcis clavier** (Entrée pour envoyer, Shift+Entrée pour nouvelle ligne)
- **🧹 Bouton d'effacement** du message
- **📱 Design responsive** et moderne
- **🌙 Support du mode sombre**

### **🔧 Props disponibles**

```jsx
<ChatInputWithCounter
  onSendMessage={handleSendMessage}    // Fonction appelée lors de l'envoi
  placeholder="Votre message..."       // Placeholder personnalisé
  maxLength={1000}                     // Limite de caractères
  showCounter={true}                   // Afficher/masquer le compteur
  disabled={false}                     // Désactiver l'input
/>
```

### **📱 Utilisation simple**

```jsx
import ChatInputWithCounter from './ChatInputWithCounter';

const MyChat = () => {
  const handleSendMessage = (message) => {
    console.log('Message envoyé:', message);
    // Votre logique d'envoi ici
  };

  return (
    <ChatInputWithCounter
      onSendMessage={handleSendMessage}
      placeholder="Tapez votre message..."
      maxLength={500}
    />
  );
};
```

---

## 🎨 Composant d'exemple : ChatExample

### **✨ Fonctionnalités**

- **💬 Interface de chat complète** avec messages utilisateur/agent
- **📊 Affichage des statistiques** de session (tokens, messages)
- **🔄 Simulation d'API** avec réponses d'agent IA
- **⏳ Indicateur de chargement** pendant la génération
- **🎯 Gestion des erreurs** avec messages d'erreur
- **📱 Design responsive** et animations

### **🔧 Utilisation**

```jsx
import ChatExample from './ChatExample';

function App() {
  return (
    <div className="App">
      <ChatExample />
    </div>
  );
}
```

---

## 🎯 Fonctionnalités du compteur

### **🌈 Changement de couleur automatique**

- **🟢 Vert (0-70%)** : Utilisation normale
- **🟡 Orange (70-90%)** : Attention, approche de la limite
- **🔴 Rouge (90-100%)** : Danger, limite presque atteinte

### **📊 Affichage en temps réel**

```
[45/500] ← Compteur avec format "utilisé/maximum"
```

### **📈 Barre de progression**

- Barre visuelle sous l'input
- Couleur change selon l'utilisation
- Animation fluide lors de la frappe

---

## 🎨 Personnalisation

### **🎨 Couleurs personnalisées**

```css
/* Personnaliser les couleurs du compteur */
.char-counter.success {
  color: #your-success-color;
  background: rgba(your-success-color, 0.1);
}

.char-counter.warning {
  color: #your-warning-color;
  background: rgba(your-warning-color, 0.1);
}

.char-counter.danger {
  color: #your-danger-color;
  background: rgba(your-danger-color, 0.1);
}
```

### **📏 Tailles personnalisées**

```css
/* Personnaliser la taille de l'input */
.chat-input {
  min-height: 60px;  /* Hauteur minimale */
  max-height: 200px; /* Hauteur maximale */
  font-size: 18px;   /* Taille de police */
}
```

---

## 🔧 Intégration avec votre API

### **📡 Remplacement de la simulation**

```jsx
// Dans ChatExample.jsx, remplacez simulateApiCall par :
const callRealAPI = async (message) => {
  const response = await fetch('http://localhost:5107/api/agent/chat', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      sessionId: currentSessionId || '',
      message: message
    })
  });
  
  return await response.json();
};
```

### **🔄 Gestion des sessions**

```jsx
const [currentSessionId, setCurrentSessionId] = useState('');

const handleSendMessage = async (messageText) => {
  try {
    const response = await callRealAPI(messageText);
    
    // Stocker l'ID de session
    if (response.sessionId) {
      setCurrentSessionId(response.sessionId);
    }
    
    // Ajouter le message à l'interface
    addMessage(response);
    
  } catch (error) {
    console.error('Erreur API:', error);
  }
};
```

---

## 📱 Responsive et accessibilité

### **📱 Mobile-first design**

- **Touch-friendly** : Boutons de taille appropriée
- **Responsive** : S'adapte à toutes les tailles d'écran
- **iOS-friendly** : Évite le zoom automatique

### **♿ Accessibilité**

- **Raccourcis clavier** : Entrée pour envoyer
- **Labels appropriés** : Titres et descriptions
- **Contraste** : Couleurs accessibles
- **Focus visible** : Indicateurs de focus clairs

---

## 🚀 Installation et utilisation

### **1. Copier les fichiers**

```bash
# Copiez tous les fichiers dans votre projet React
cp ChatInputWithCounter.jsx src/components/
cp ChatInputWithCounter.css src/components/
cp ChatExample.jsx src/components/
cp ChatExample.css src/components/
```

### **2. Importer dans votre app**

```jsx
import ChatInputWithCounter from './components/ChatInputWithCounter';
import ChatExample from './components/ChatExample';
```

### **3. Utiliser le composant**

```jsx
function App() {
  return (
    <div className="App">
      <ChatExample />
    </div>
  );
}
```

---

## 🎯 Cas d'usage recommandés

### **💬 Chat simple**
- Utilisez `ChatInputWithCounter` seul
- Gestion basique des messages

### **🤖 Chat avec agent IA**
- Utilisez `ChatExample` complet
- Intégration avec votre API MultiAgentsBeta
- Affichage des statistiques de session

### **📱 Application mobile**
- Composants déjà optimisés mobile
- Support tactile et responsive

---

## 🔍 Dépannage

### **❌ Compteur ne s'affiche pas**

```jsx
// Vérifiez que showCounter est true
<ChatInputWithCounter
  showCounter={true}  // ← Doit être true
  onSendMessage={handleSend}
/>
```

### **❌ Styles ne s'appliquent pas**

```jsx
// Assurez-vous d'importer le CSS
import './ChatInputWithCounter.css';
```

### **❌ Compteur ne se met pas à jour**

```jsx
// Vérifiez que onSendMessage est une fonction
const handleSend = (message) => {
  console.log('Message:', message);
};

<ChatInputWithCounter onSendMessage={handleSend} />
```

---

## 🎉 Résultat final

Vous obtenez une interface de chat moderne avec :

✅ **Compteur de caractères** en temps réel  
✅ **Changement de couleur** selon l'utilisation  
✅ **Design responsive** et moderne  
✅ **Intégration facile** avec votre API  
✅ **Support mobile** et accessibilité  
✅ **Animations fluides** et UX optimisée  

**🚀 Prêt pour la production !**

