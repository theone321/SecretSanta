SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;


\connect santa


-- Names
INSERT INTO "Names"  ("RegisteredName", "IsAdmin") VALUES ('Michael Marvin', TRUE);
INSERT INTO "Names" ("RegisteredName", "IsAdmin") VALUES ('Tobias Becker', TRUE);
INSERT INTO "Names" ("RegisteredName") VALUES ('Sarah Marvin-Foley');
INSERT INTO "Names" ("RegisteredName") VALUES ('Angelia Becker');
INSERT INTO "Names" ("RegisteredName") VALUES ('Jonathon Minelli');
INSERT INTO "Names" ("RegisteredName") VALUES ('Sarah Leahman');
INSERT INTO "Names" ("RegisteredName") VALUES ('Amanda Robinson');
INSERT INTO "Names" ("RegisteredName") VALUES ('Caleb Gaffney');
INSERT INTO "Names" ("RegisteredName") VALUES ('Dale Banas');
INSERT INTO "Names" ("RegisteredName") VALUES ('Dorothy Klein');
INSERT INTO "Names" ("RegisteredName") VALUES ('Lindsay Shockling');
INSERT INTO "Names" ("RegisteredName") VALUES ('Steve Rakar');
INSERT INTO "Names" ("RegisteredName") VALUES ('Andrew Sansone');
INSERT INTO "Names" ("RegisteredName") VALUES ('Heather Sansone');

SELECT * FROM "Names";

-- MatchRestrictions

INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Tobias Becker', 'Angelia Becker');
    
INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Angelia Becker', 'Tobias Becker');

    
INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Michael Marvin', 'Sarah Marvin-Foley');
    
INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Sarah Marvin-Foley', 'Michael Marvin');

    
INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Dale Banas', 'Dorothy Klein');

INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Dorothy Klein', 'Dale Banas');


INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Amanda Robinson', 'Caleb Gaffney');

INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Caleb Gaffney', 'Amanda Robinson');


INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Jonathon Minelli', 'Lindsay Shockling');

INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Lindsay Shockling', 'Jonathon Minelli');

    
INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Andrew Sansone', 'Heather Sansone');

INSERT INTO "MatchRestrictions" ("RequestorName", "RestrictedName") 
    VALUES ('Heather Sansone', 'Andrew Sansone');
    
-- Settings
INSERT INTO "Settings" VALUES ('AllowRegistration', 'true');
INSERT INTO "Settings" VALUES ('AllowMatching', 'false');
INSERT INTO "Settings" VALUES ('SessionTimeout', '15');