using UnityEngine;

/// <summary>
/// Dokumentace všech artefaktů ve hře
/// Obsahuje návrhy efektů pro každou epochu
/// 
/// PRAVĚK (Prehistoric):
/// 1. Ooga Booga - Zvýšení damage
/// 2. Sběr Bobulí (Berry Gathering) - Zvýšení regenerace
/// 3. Síla Mamuta (Mammoth Strength) - Zvýšení max health
/// 
/// STAROVĚK (Ancient):
/// 4. Sparťanský Štít - Zvýšení defense
/// 5. Olympijský Oheň - Zvýšení rychlosti
/// 6. Filosof Kámen - Kritický hit chance
/// 
/// STŘEDOVĚK (Medieval):
/// 7. Excalibur Fragment - Masivní damage boost
/// 8. Dračí Šupina - Fire resistance + defense
/// 9. Rytířská Ctnost - Zvýšení attack speed
/// 
/// BUDOUCNOST (Future):
/// 10. Nano Reaktor - Life steal + regenerace
/// 11. Kvantový Chip - Dash recharge rychlost
/// 12. Plazmový Katalyzátor - Extreme damage + speed
/// </summary>
public class ArtifactDatabase : MonoBehaviour
{
    [Header("Pravěk Artefakty")]
    [Tooltip("Ooga Booga - Zvýšení poškození")]
    public ArtifactData oogaBooga;
    
    [Tooltip("Sběr Bobulí - Zvýšení regenerace")]
    public ArtifactData sberBobuli;
    
    [Tooltip("Síla Mamuta - Zvýšení max health")]
    public ArtifactData silaMamuta;
    
    [Header("Starověk Artefakty")]
    [Tooltip("Sparťanský Štít - Zvýšení defense")]
    public ArtifactData spartanskyStit;
    
    [Tooltip("Olympijský Oheň - Zvýšení rychlosti")]
    public ArtifactData olympijskyOhen;
    
    [Tooltip("Filosof Kámen - Kritický hit chance")]
    public ArtifactData filosofKamen;
    
    [Header("Středověk Artefakty")]
    [Tooltip("Excalibur Fragment - Masivní damage boost")]
    public ArtifactData excaliburFragment;
    
    [Tooltip("Dračí Šupina - Defense boost")]
    public ArtifactData draciSupina;
    
    [Tooltip("Rytířská Ctnost - Attack speed")]
    public ArtifactData rytirskaCtvnost;
    
    [Header("Budoucnost Artefakty")]
    [Tooltip("Nano Reaktor - Life steal + regenerace")]
    public ArtifactData nanoReaktor;
    
    [Tooltip("Kvantový Chip - Dash recharge")]
    public ArtifactData kvantovyChip;
    
    [Tooltip("Plazmový Katalyzátor - Extreme stats")]
    public ArtifactData plazmovyKatalyzator;
}

/*
 * NÁVOD NA VYTVOŘENÍ ARTEFAKTŮ V UNITY:
 * 
 * 1. V Unity editoru, klikni pravým tlačítkem v Project okně
 * 2. Create > Relics of Tomorrow > Artifact
 * 3. Pojmenuj artefakt (např. "Ooga_Booga")
 * 4. Nastav parametry podle níže uvedených hodnot
 * 
 * ==========================================
 * PRAVĚK ARTEFAKTY
 * ==========================================
 * 
 * OOGA BOOGA:
 * - Artifact Name: "Ooga Booga"
 * - Description: "Prastaré bojové heslo tvých předků. Zvyšuje sílu tvých úderů o 50%!"
 * - Epoch: Pravek
 * - Effect Type: DamageBoost
 * - Effect Value: 1.5 (= +50% damage)
 * - Aura Color: Oranžová (1, 0.5, 0, 0.3)
 * 
 * SBĚR BOBULÍ:
 * - Artifact Name: "Sběr Bobulí"
 * - Description: "Znalost prastarých léčivých bobulí. Tvé zdraví se regeneruje 3x rychleji!"
 * - Epoch: Pravek
 * - Effect Type: RegenBoost
 * - Effect Value: 3.0 (= 3x rychlejší regenerace)
 * - Aura Color: Zelená (0.2, 1, 0.2, 0.3)
 * 
 * SÍLA MAMUTA:
 * - Artifact Name: "Síla Mamuta"
 * - Description: "Energie mocného mamuta proudí tvým tělem. Zvyšuje maximální zdraví o 50 bodů!"
 * - Epoch: Pravek
 * - Effect Type: HealthBoost
 * - Effect Value: 50 (= +50 HP)
 * - Secondary Effect Type: DefenseBoost
 * - Secondary Effect Value: 1.2 (= +20% defense)
 * - Aura Color: Hnědá (0.6, 0.4, 0.2, 0.3)
 * 
 * ==========================================
 * STAROVĚK ARTEFAKTY
 * ==========================================
 * 
 * SPARŤANSKÝ ŠTÍT:
 * - Artifact Name: "Sparťanský Štít"
 * - Description: "Legendární štít spartánských bojovníků. Snižuje přijímané poškození o 40%!"
 * - Epoch: Starovek
 * - Effect Type: DefenseBoost
 * - Effect Value: 1.4 (= -40% damage taken = 1/(1-0.4))
 * - Aura Color: Bronzová (0.8, 0.5, 0.2, 0.3)
 * 
 * OLYMPIJSKÝ OHEŇ:
 * - Artifact Name: "Olympijský Oheň"
 * - Description: "Plamen olympijských her hoří ve tvé duši. Zvyšuje rychlost pohybu o 40%!"
 * - Epoch: Starovek
 * - Effect Type: SpeedBoost
 * - Effect Value: 1.4 (= +40% speed)
 * - Secondary Effect Type: StaminaBoost
 * - Secondary Effect Value: 1.3 (= +30% stamina)
 * - Aura Color: Oranžovo-zlatá (1, 0.7, 0.1, 0.3)
 * 
 * FILOSOF KÁMEN:
 * - Artifact Name: "Filosof Kámen"
 * - Description: "Mystická alchymistická esence. Tvé útoky mají 25% šanci na kritický zásah!"
 * - Epoch: Starovek
 * - Effect Type: CriticalChance
 * - Effect Value: 0.25 (= 25% crit chance)
 * - Secondary Effect Type: DamageBoost
 * - Secondary Effect Value: 1.15 (= +15% damage)
 * - Aura Color: Fialová (0.7, 0.3, 1, 0.3)
 * 
 * ==========================================
 * STŘEDOVĚK ARTEFAKTY
 * ==========================================
 * 
 * EXCALIBUR FRAGMENT:
 * - Artifact Name: "Fragment Excaliburu"
 * - Description: "Úlomek legendárního meče krále Artuše. Tvá síla je neuvěřitelná! +80% damage!"
 * - Epoch: Stredovek
 * - Effect Type: DamageBoost
 * - Effect Value: 1.8 (= +80% damage)
 * - Secondary Effect Type: CriticalChance
 * - Secondary Effect Value: 0.15 (= 15% crit)
 * - Aura Color: Světle modrá (0.5, 0.7, 1, 0.3)
 * 
 * DRAČÍ ŠUPINA:
 * - Artifact Name: "Dračí Šupina"
 * - Description: "Šupina prastarého draka. Tvá kůže je tvrdá jako ocel. +50% defense!"
 * - Epoch: Stredovek
 * - Effect Type: DefenseBoost
 * - Effect Value: 1.5 (= +50% defense)
 * - Secondary Effect Type: HealthBoost
 * - Secondary Effect Value: 30 (= +30 HP)
 * - Aura Color: Tmavě červená (0.8, 0.1, 0.1, 0.3)
 * 
 * RYTÍŘSKÁ CTNOST:
 * - Artifact Name: "Rytířská Ctnost"
 * - Description: "Duch pravého rytíře vede tvou ruku. Útočíš o 60% rychleji!"
 * - Epoch: Stredovek
 * - Effect Type: AttackSpeedBoost
 * - Effect Value: 1.6 (= +60% attack speed)
 * - Secondary Effect Type: SpeedBoost
 * - Secondary Effect Value: 1.25 (= +25% movement)
 * - Aura Color: Zlatá (1, 0.85, 0.3, 0.3)
 * 
 * ==========================================
 * BUDOUCNOST ARTEFAKTY
 * ==========================================
 * 
 * NANO REAKTOR:
 * - Artifact Name: "Nano Reaktor"
 * - Description: "Nano-technologie opravují tvé tělo v reálném čase. Kradeš 20% zdraví z poškození!"
 * - Epoch: Budoucnost
 * - Effect Type: LifeSteal
 * - Effect Value: 0.2 (= 20% life steal)
 * - Secondary Effect Type: RegenBoost
 * - Secondary Effect Value: 2.5 (= 2.5x regen)
 * - Aura Color: Kybernetická zelená (0.2, 1, 0.6, 0.3)
 * 
 * KVANTOVÝ CHIP:
 * - Artifact Name: "Kvantový Chip"
 * - Description: "Kvantový procesor zrychluje tvé reflexy. Dash se nabíjí 50% rychleji!"
 * - Epoch: Budoucnost
 * - Effect Type: DashRecharge
 * - Effect Value: 1.5 (= 1.5x faster)
 * - Secondary Effect Type: SpeedBoost
 * - Secondary Effect Value: 1.3 (= +30% speed)
 * - Aura Color: Modrá neon (0.3, 0.6, 1, 0.3)
 * 
 * PLAZMOVÝ KATALYZÁTOR:
 * - Artifact Name: "Plazmový Katalyzátor"
 * - Description: "Ultimátní technologie budoucnosti. Masivní boost všech statistik!"
 * - Epoch: Budoucnost
 * - Effect Type: DamageBoost
 * - Effect Value: 2.0 (= +100% damage!!!)
 * - Secondary Effect Type: SpeedBoost
 * - Secondary Effect Value: 1.5 (= +50% speed)
 * - Aura Color: Elektrická purpurová (1, 0.3, 1, 0.3)
 */
