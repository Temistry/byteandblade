using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using CCGKit;
using System.Text;
using Newtonsoft.Json;

namespace CCGKit
{
    public class DeepInfraAPI : MonoBehaviour
    {
        private const string API_KEY = "hTMFGe4TaeXrZqA664KtdssyciKnxfbO";
        private const string API_URL = "https://api.deepinfra.com/v1/openai/chat/completions";
        private const string MODEL = "meta-llama/Meta-Llama-3-8B-Instruct";

        [Serializable]
        private class ChatMessage
        {
            [JsonProperty("role")]
            public string Role { get; set; }
            
            [JsonProperty("content")]
            public string Content { get; set; }
        }

        [Serializable]
        private class ChatRequest
        {
            [JsonProperty("model")]
            public string Model { get; set; }
            
            [JsonProperty("messages")]
            public List<ChatMessage> Messages { get; set; }
            
            [JsonProperty("temperature")]
            public float Temperature { get; set; }
            
            [JsonProperty("max_tokens")]
            public int MaxTokens { get; set; }
        }

        [Serializable]
        private class ChatResponse
        {
            [JsonProperty("choices")]
            public List<Choice> Choices { get; set; }
        }

        [Serializable]
        private class Choice
        {
            [JsonProperty("message")]
            public ChatMessage Message { get; set; }
        }

        public string GeneratePattern(EnemyTemplate enemyTemplate, HeroTemplate playerTemplate, int numPatterns)
        {
            try
            {
                var prompt = GeneratePrompt(enemyTemplate, playerTemplate, numPatterns);
                var messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "system", Content = "You are a game designer creating enemy patterns for a card game. You must analyze the player's current state and create appropriate patterns to challenge them." },
                    new ChatMessage { Role = "user", Content = prompt }
                };

                var request = new ChatRequest
                {
                    Model = MODEL,
                    Messages = messages,
                    Temperature = 0.5f,
                    MaxTokens = 1000
                };

                string response = SendRequest(request);
                return response;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating pattern: {e.Message}");
                return null;
            }
        }

        private string GeneratePrompt(EnemyTemplate enemyTemplate, HeroTemplate playerTemplate, int numPatterns)
        {
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine($"Generate {numPatterns} unique and aggressive patterns for an enemy with the following characteristics:");
            prompt.AppendLine($"- Name: {enemyTemplate.name}");
            prompt.AppendLine($"- Is Boss: {enemyTemplate.IsBoss}");
            prompt.AppendLine($"- Is Elite: {enemyTemplate.IsElite}");
            prompt.AppendLine($"- HP Range: {enemyTemplate.HpLow} - {enemyTemplate.HpHigh}");
            
            prompt.AppendLine("\nCurrent Player State:");
            prompt.AppendLine($"- Player Name: {playerTemplate.name}");
            prompt.AppendLine($"- Current Health: {playerTemplate.Health} / {playerTemplate.MaxHealth}");
            prompt.AppendLine($"- Current Mana: {playerTemplate.Mana}");
            prompt.AppendLine($"- Starting Deck Size: {playerTemplate.StartingDeck.Entries.Count}");
            prompt.AppendLine($"- Reward Deck Size: {playerTemplate.RewardDeck.Entries.Count}");
            
            prompt.AppendLine("\nEach pattern should:");
            prompt.AppendLine("- Create high tension and pressure for the player");
            prompt.AppendLine("- Include aggressive damage dealing effects");
            prompt.AppendLine("- Consider using status effects to limit player options");
            prompt.AppendLine("- Include defensive effects to sustain the enemy");
            prompt.AppendLine("- Create a sense of urgency and danger");
            prompt.AppendLine("- Force the player to make difficult decisions");
            prompt.AppendLine("- Include a unique name and description that reflects the aggressive nature");
            prompt.AppendLine("- Specify effects with types, values, targets, and durations");
            prompt.AppendLine("- Be challenging but fair based on the player's current state");
            
            prompt.AppendLine("\nAllowed Effect Types:");
            prompt.AppendLine("- Damage: Deal damage to target");
            prompt.AppendLine("- Shield: Gain shield points");
            prompt.AppendLine("- Weak: Apply Weak status (reduces damage dealt)");
            prompt.AppendLine("- Strength: Apply Strength status (increases damage dealt)");
            prompt.AppendLine("- Poison: Apply Poison status (deals damage over time)");
            prompt.AppendLine("- Heal: Restore HP");
            
            prompt.AppendLine("\nTarget Rules:");
            prompt.AppendLine("- All effects must target the player (Galahad)");
            prompt.AppendLine("- No effects should target the enemy itself");
            prompt.AppendLine("- The target field must always be \"Galahad\"");
            
            prompt.AppendLine("\nReturn ONLY the JSON response with the following structure:");
            prompt.AppendLine("{");
            prompt.AppendLine("  \"patterns\": [");
            prompt.AppendLine("    {");
            prompt.AppendLine("      \"name\": \"Pattern Name\",");
            prompt.AppendLine("      \"description\": \"Pattern Description\",");
            prompt.AppendLine("      \"effects\": [");
            prompt.AppendLine("        {");
            prompt.AppendLine("          \"type\": \"Damage\",");
            prompt.AppendLine("          \"value\": 10,");
            prompt.AppendLine("          \"target\": \"Galahad\",");
            prompt.AppendLine("          \"duration\": 1");
            prompt.AppendLine("        }");
            prompt.AppendLine("      ]");
            prompt.AppendLine("    }");
            prompt.AppendLine("  ]");
            prompt.AppendLine("}");
            
            prompt.AppendLine("\nExample response:");
            prompt.AppendLine("{");
            prompt.AppendLine("  \"patterns\": [");
            prompt.AppendLine("    {");
            prompt.AppendLine("      \"name\": \"Toxic Tide\",");
            prompt.AppendLine("      \"description\": \"A deadly combination of poison and damage\",");
            prompt.AppendLine("      \"effects\": [");
            prompt.AppendLine("        {");
            prompt.AppendLine("          \"type\": \"Damage\",");
            prompt.AppendLine("          \"value\": 8,");
            prompt.AppendLine("          \"target\": \"Galahad\",");
            prompt.AppendLine("          \"duration\": 1");
            prompt.AppendLine("        },");
            prompt.AppendLine("        {");
            prompt.AppendLine("          \"type\": \"Poison\",");
            prompt.AppendLine("          \"value\": 2,");
            prompt.AppendLine("          \"target\": \"Galahad\",");
            prompt.AppendLine("          \"duration\": 2");
            prompt.AppendLine("        }");
            prompt.AppendLine("      ]");
            prompt.AppendLine("    }");
            prompt.AppendLine("  ]");
            prompt.AppendLine("}");
            
            prompt.AppendLine("\nImportant rules:");
            prompt.AppendLine("1. The response must be a valid JSON object");
            prompt.AppendLine("2. The response must start with { and end with }");
            prompt.AppendLine("3. The response must contain a \"patterns\" array");
            prompt.AppendLine("4. Each pattern must have a name, description, and effects array");
            prompt.AppendLine("5. Each effect must have type, value, target, and duration");
            prompt.AppendLine("6. Do not include any text before or after the JSON");
            
            return prompt.ToString();
        }

        private string SendRequest(ChatRequest request)
        {
            string jsonRequest = JsonConvert.SerializeObject(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

            using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                {
                    // 동기적으로 대기
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonConvert.DeserializeObject<ChatResponse>(webRequest.downloadHandler.text);
                    var content = response?.Choices?[0]?.Message?.Content;
                    
                    // JSON 응답만 추출
                    if (content != null)
                    {
                        int startIndex = content.IndexOf('{');
                        int endIndex = content.LastIndexOf('}') + 1;
                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            return content.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                    return null;
                }
                else
                {
                    Debug.LogError($"API request failed: {webRequest.error}");
                    return null;
                }
            }
        }
    }
} 