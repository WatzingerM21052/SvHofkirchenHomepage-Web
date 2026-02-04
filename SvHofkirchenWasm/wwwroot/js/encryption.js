window.encryptionHelper = {
    // Leitet einen AES-256 Schlüssel aus einem Passwort ab (PBKDF2)
    deriveKey: async (password) => {
        const encoder = new TextEncoder();
        const pwKey = await crypto.subtle.importKey(
            "raw", encoder.encode(password), "PBKDF2", false, ["deriveKey"]
        );
        
        // Der 'Salt' sorgt dafür, dass gleiche Passwörter unterschiedliche Schlüssel ergeben
        const salt = encoder.encode("SvHofkirchen_Super_Safe_Salt_2026");

        return await crypto.subtle.deriveKey(
            { name: "PBKDF2", salt: salt, iterations: 100000, hash: "SHA-256" },
            pwKey,
            { name: "AES-GCM", length: 256 },
            false, ["encrypt", "decrypt"]
        );
    },

    encryptData: async (plainText, password) => {
        const encoder = new TextEncoder();
        const key = await window.encryptionHelper.deriveKey(password);
        const iv = crypto.getRandomValues(new Uint8Array(12)); // Zufälliger Start-Vektor
        const encrypted = await crypto.subtle.encrypt(
            { name: "AES-GCM", iv: iv },
            key, encoder.encode(plainText)
        );

        // IV und Daten zusammenfügen und als Base64 senden
        const combined = new Uint8Array(iv.length + encrypted.byteLength);
        combined.set(iv);
        combined.set(new Uint8Array(encrypted), iv.length);
        return btoa(String.fromCharCode.apply(null, combined));
    },

    decryptData: async (base64Data, password) => {
        try {
            const key = await window.encryptionHelper.deriveKey(password);
            const combined = new Uint8Array(atob(base64Data).split("").map(c => c.charCodeAt(0)));
            const iv = combined.slice(0, 12);
            const data = combined.slice(12);

            const decrypted = await crypto.subtle.decrypt(
                { name: "AES-GCM", iv: iv },
                key, data
            );
            return new TextDecoder().decode(decrypted);
        } catch (e) {
            console.error("Entschlüsselung fehlgeschlagen - Passwort falsch?");
            return null;
        }
    }
};