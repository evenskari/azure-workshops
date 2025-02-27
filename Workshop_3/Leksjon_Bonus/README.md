# Bonus Leksjon: Sikre infrastruktur

Nå skal du ha en sikker applikasjon som tillater kun en innlogget bruker å lagre bilder. Et problem som fortsatt eksisterer er at Storage kontoen kan være tilgjengelig for alle over internett. Derfor kan observante personer få tilgang til bilder du laster opp uten å være innlogget. Under veiviseren, i portalen, når du lager en Storage Account så får du muligheten til å sette Connectivity method (under Networking), her kan man sette Public endpoint (all networks), Public networks (selected networks) og private endpoint. Som oftest velger man Public endpoint (all networks) fordi det er raskest å få til å fungere, men dette gjør at

---

Før vi låser ned Storage Accounten så er det viktig å sjekke at applikasjonen er klar. I StorageService finnes det en metode som heter GetImageUrls, sjekk at denne lager en SharedAccessSignature.

For å sikre Storage Accounten så må vi gå i portalen og sjekke/endre litt

- Pass på at container er private access. Dette finner du under Container sin Access level. - Finn Storage Accounten i portalen. - Trykk Containers, finn containeren du vil endre på og trykk på den. - Ovenfor alle filene vil det være et par valg, sjekk her at containeren har access level "Private (no anonymous access)". Trykk OK,

- Gå tilbake til Storage Accounten - På venstre side så skal det finnes et valg som heter Networking. - Her inne legger du til et virtuelt nettverk. - Hvis du ikke har et fra før så kan du lage det gjennom å velge "Selected Networks" og "Add new virtual network" valget. Gi nettet et navn, og la bare de anbefalte verdiene stå. 
- Det kan hende du må fylle ut en Address range for ditt subnet. Du fyller da ut den samme adressen som du har for Address space, men erstatter tallet etter "/" med 29.
- Trykk på "Create".

---

I tillegg må du gi Web App'en din mulighet til å snakke med Storage Accounten din, nå når denne ligger ligger i et VNET.

- Gå til Web App'en din i portalen.
- VNET integrasjon er en Standard-tier feature i App Service Plan. Skifte App Service Plan: Trykk på "Scale Up". Velg så "S1" under "Production".
- Gå så til Networking i menyen til venstre. Trykk "VNET integration".
- Trykk "+ Add VNET (preview)" og velg det virtuelle nettverket du opprettet i forrige. Det har bare et subnet, så velg dette. Trykk OK. Nå har Web App'en din lov til å

Test så at applikasjonen din ikke fungerer lengre, og bildene vises. Hvorfor ikke? Du har jo SAS-token?
Grunnen til dette er at nå er det kun Web App'en (backend) som har lov til å snakke direkte med Storage Account i VNET, mens Web App'en kun returnerer linker (med SAS-token) direkte til Storage Account. Per nå
har du ikke tilgang til Storage Account fra din adresse.

Hvis du fulgte litt godt med så så du kanskje valget med Firewall og legg til din IP-addresse (under "Firewalls and networks settings" på Storage Account)? Huk av for den og trykk lagre på nytt. Nå skal applikasjonen din fungere igjen. Valget med IP-addressen finnes i Storage Account under Firewalls and Virtual Networks. Gå til vnet og se litt rundt på valg. Her kan man parre med andre nettverk, legge til devices, opprette/endre subnets osv.

- Lag så en Network Security Group, som du så legger på VNettet du opprettet ovenfor. 
- Dette kan gjøres ved å søke etter Network Security Group i søkefeltet eller via + tegnet på venstre side i menyen. 
- Opprett så en Network Security Group, pass på at du oppretter i samme ressursgruppe og region som det virituelle nettverket du lagde. 
- Etter at NSG er opprettet, gå til ressursen. 
- Gå til subnets (på venstre side i ressursbildet) og trykk Associate. - Her velger du det virtuelle nettverke du valgte og subnettet som Storage Accounten ligger på.

Nå har du muligheten til å velge hvem som har tilgang til alle ressursene som ligger i det subnettet.
Man kan gjøre det mulig å kontakte Storage Accounten fra spesifikke tjenester som ligger i vnettet, f.eks. virtuelle maskiner.
Ettersom det tar litt tid før endringene du gjør i NSGen faktisk blir aktive, så anbefales det at man venter så mye som 20 minutter før du prøver en annen regel.

For en applikasjon sånn som dette, så ville det vært lurt å ikke bruke virtuelt nettverk på Storage kontoen, men det finnes mange scenarioer der du ikke vil at dataene dine skal kunne komme på utsiden av dine applikasjoner og da er slike restriksjoner veldig fine å bruke. Da er det bare å sette opp VNET og NSG og så få på plass reglene man trenger for sin arkitektur.
