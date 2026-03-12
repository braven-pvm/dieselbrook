
DEFINE CLASS Campain AS custom

    cerrormsg = ""
    lerror = .F.
    Name = "ajaxcallbacks"

    */--------------------------------------------------------------------------------
    *  Get Brands
    */--------------------------------------------------------------------------------
    PROCEDURE GetBrands
        LPARAM lctype

        IF ISNULLOREMPTY(lctype)
            RETURN ""
        ENDIF

        lcFilter = ""
        lcFilter = " and i.ctype='" + lctype + "'"

        TEXT TO lcSql TEXTMERGE NOSHOW
        Select DISTINCT s.cCode,s.cDescript from icitem i
        join Comisc s  on  s.cType='ITEMCLASS' and s.ccode=i.cclass
        WHERE i.cStatus='A' <<lcFilter>>
        order by s.cdescript
        ENDTEXT

        IF oAMsql.execute(lcSql, 'Tquery') = 1
            RETURN "cursor:TQuery"
        ELSE
            RETURN ""
        ENDIF

        RETURN
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Summary
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSummary
        LPARAMETERS ldfrom, ldto, lnCampID

        IF !EMPTY(lnCampID)
            lcFilter = "@campId=" + TRANSFORM(lnCampID)
        ELSE
            lcFilter = "@start ='" + ldfrom + "',@end='" + ldto + "'"
        ENDIF

        luret = oAMSQL.Execute("EXEC sp_Camp_CatSummary " + lcFilter, "TCamp")
        IF luret < 1
            RETURN .F.
        ENDIF

        loCamp = CursorToCollection("TCamp")
        loCategory = CursorToCollection("TCamp1")
        loSponcat = CursorToCollection("TCamp2")
        loBrand = CursorToCollection("TCamp3")

        FOR xx = 1 TO loCamp.Count
            loCamp[xx].ocat = CREATEOBJECT("Collection")
            FOR ii = 1 TO loCategory.Count
                IF loCategory[ii].campaignid = loCamp[xx].id
                    loCamp[xx].ocat.Add(loCategory[ii])
                ENDIF
            NEXT
        NEXT

        FOR xx = 1 TO loCamp.Count
            loCamp[xx].ospon = CREATEOBJECT("Collection")
            FOR ii = 1 TO loSponcat.Count
                IF loSponcat[ii].campaignid = loCamp[xx].id
                    loCamp[xx].ospon.Add(loSponcat[ii])
                ENDIF
            NEXT
        NEXT

        FOR xx = 1 TO loCamp.Count
            loCamp[xx].obrand = CREATEOBJECT("Collection")
            FOR ii = 1 TO loBrand.Count
                IF loBrand[ii].campaignid = loCamp[xx].id
                    loCamp[xx].obrand.Add(loBrand[ii])
                ENDIF
            NEXT
        NEXT

        FOR xx = 1 TO loCamp.Count
            loCamp[xx].odash = CREATEOBJECT("Collection")

            lod = CREATEOBJECT("EMPTY")
            ADDPROPERTY(lod, "label", "Target")
            ADDPROPERTY(lod, "tot", 0)
            FOR ii = 1 TO 6
                TRY
                    ADDPROPERTY(lod, "C" + TRANSFORM(ii), loCamp[xx].ocat[ii].nTarget)
                    lod.tot = lod.tot + loCamp[xx].ocat[ii].nTarget
                CATCH
                    ADDPROPERTY(lod, "C" + TRANSFORM(ii), 0)
                ENDTRY
            NEXT
            FOR ii = 1 TO 4
                ADDPROPERTY(lod, "S" + TRANSFORM(ii), loCamp[xx].ospon[ii].nTarget)
            NEXT
            loCamp[xx].odash.Add(lod)
        NEXT

        RETURN loCamp
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Sku By Month
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSkuByMonth
        luret = oAMSQL.Execute("EXEC sp_Camp_SkuByMonth @Category='SKINCARE', @start ='2022-01-01',@end ='2022-12-01'", "TCamp")
        loCamp = CursorToCollection("TCamp")

        IF luret < 1
            RETURN .F.
        ENDIF

        RETURN loCamp
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Sku By Month Vert
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSkuByMonthVert
        LPARAMETERS lcCat, ldstart, ldend

        luret = oAMSQL.Execute("EXEC sp_Camp_SkuByMonthvert @Category='" + lcCat + "', @start ='" + ldStart + "',@end ='" + ldend + "'", "TCamp")
        IF luret < 1
            RETURN .F.
        ENDIF

        RETURN this.CampGetSku()
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Brand By Month
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetBrandByMonth
        luret = oAMSQL.Execute("EXEC sp_Camp_BrandByMonth @Category='SKINCARE', @start ='2022-01-01',@end ='2022-12-01'", "TCamp")
        loBrand = CursorToCollection("TCamp")

        IF luret < 1
            RETURN .F.
        ENDIF

        RETURN loBrand
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Update
    */--------------------------------------------------------------------------------
    PROCEDURE CampUpdate
        LPARAMETERS lcAction, oData
        poError = CREATEOBJECT("HtmlErrorDisplayConfig")

        lo = NEWOBJECT("campaign", "campdata")
        lo.SetSqlObject(oAMSql)

        loC = NEWOBJECT("CampCat", "CampData")
        loC.SetSqlObject(oAMSql)

        loS = NEWOBJECT("CampSponSum", "CampData")
        loS.SetSqlObject(oAMSql)

        loB = NEWOBJECT("CampBrand", "CampData")
        loB.SetSqlObject(oAMSql)

        IF VARTYPE(oData) <> "O"
            poError.Errors.AddError("Invalid Data Submitted")
            RETURN poError
        ENDIF

        IF PEMSTATUS(oData, "id", 5)
            pnId = oData.ID
        ELSE
            pnId = 0
        ENDIF

        DO CASE
            CASE lcAction = "UPDATE"
                IF !lo.load(pnId)
                    poError.Errors.AddError("Could not update")
                    RETURN poError
                ENDIF

                =copyobjectproperties(oData, lo.oData, 2)
                lo.oData.lastuser = process.cauthenticateduser
                lo.oData.dlastupdate = DATETIME()

                IF !lo.Save()
                    poError.Errors.AddError("Could not save campaign")
                    RETURN poError
                ENDIF

                RETURN this.CampGetSummary(,, pnId)
        ENDCASE

        RETURN
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Update Sku
    */--------------------------------------------------------------------------------
    PROCEDURE CampUpdateSku
        LPARAMETERS lcAction, oData
        LOCAL lo, loC

        poError = CREATEOBJECT("HtmlErrorDisplayConfig")

        loCamp = NEWOBJECT("Campaign", "CampData")
        loCamp.SetSqlObject(oAMSql)

        lo = NEWOBJECT("CampSku", "CampData")
        lo.SetSqlObject(oAMSql)

        loC = NEWOBJECT("CampDetail", "CampData")
        loC.SetSqlObject(oAMSql)

        IF VARTYPE(oData) <> "O"
            poError.Errors.AddError("Invalid Data Submitted")
            RETURN poError
        ENDIF

        RETURN .T.
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Camps
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetCamps
        luret = oAMSQL.Execute("EXEC sp_Camp_GetCamp", "TCamp")
        loCamp = CursorToCollection("TCamp")

        IF luret < 1
            RETURN .F.
        ENDIF

        RETURN loCamp
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Get Items
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetItems
        LPARAM srch, noKits
        codeonly = .T.

        IF ISNULLOREMPTY(srch)
            RETURN ""
        ENDIF

        IF LEFT(srch, 1) = "*"
            IF LEN(srch) < 3
                RETURN ""
            ENDIF
            lcFilter = "(cNPDCode like '" + LTRIM(srch) + "%' )"

            TEXT TO lcSql TEXTMERGE NOSHOW
            Select DISTINCT i.ctype,i.cclass,cNPDCode cItemno,i.cDescript,nCost nstdcost,
            i.ctype2,i.ctype3,i.ctype1,i.cbuyer,i.cprodline,
            nprcinctx,ROUND(nprcinctx/(1+(goSettings.common.vatrate/100)),4) nprice,1 lPending from NPDPending i
            WHERE  <<lcFilter>>
            order by  i.ctype,i.cclass,i.cdescript
            ENDTEXT
        ELSE
            lcFilter = "(ctype<>'ARCHIVE' and citemno like '" + LTRIM(srch) + "%' )" + ;
                IIF(!EMPTY(noKits), " and lkititem=0 ", "")

            TEXT TO lcSql TEXTMERGE NOSHOW
            Select DISTINCT i.ctype,i.cclass,i.cItemno,i.cDescript,i.nstdcost,
            i.ctype2,i.ctype3,i.ctype1,i.cbuyer,i.cprodline,i.nprcinctx,i.nprice,0 lPending
             from icitem i
            WHERE  <<lcFilter>>
            order by  i.ctype,i.cclass,i.cdescript
            ENDTEXT
        ENDIF

        IF oAMsql.execute(lcSql, 'Tquery') = 1
            RETURN "cursor:TQuery"
        ELSE
            RETURN ""
        ENDIF

        RETURN
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Spon Types
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSponTypes
        luret = oAMSQL.Execute("EXEC sp_Camp_GetSponTypes", "TSpon")
        IF luret < 1
            RETURN .F.
        ENDIF
        loSponType = CursorToCollection("TSpon")
        loSponCat = CursorToCollection("TSpon1")
        loSpon = CREATEOBJECT("Empty")
        ADDPROPERTY(loSpon, "SponTypes", loSponType)
        ADDPROPERTY(loSpon, "SponCats", loSponCat)

        RETURN loSpon
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Sku
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSku
        loSku = CursorToCollection("TCamp")

        IF luret < 1
            RETURN .F.
        ENDIF

        loDetail = CursorToCollection("TCamp1")

        FOR xx = 1 TO loSku.Count
            loSku[xx].odetail = CREATEOBJECT("Collection")
            FOR ii = 1 TO loDetail.Count
                IF loDetail[ii].campskuid = loSku[xx].id
                    loSku[xx].odetail.Add(loDetail[ii])
                ENDIF
            NEXT
        NEXT

        RETURN loSku
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Item Lookups
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetItemLookups
        poError = CREATEOBJECT("HtmlErrorDisplayConfig")
        loSet = CREATEOBJECT("csettings")
        loSet.setsqlobject(oSql)
        loSetting = CREATEOBJECT("EMPTY")
        =loSet.LoadUserSettings(loSetting, "camp")
        IF !PEMSTATUS(loSetting, "camp", 5)
            poError.Errors.AddError("Not Authenticated")
            RETURN poError
        ENDIF

        RETURN .T.
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Settings
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSettings
        loSet = CREATEOBJECT("csettings")
        loSet.setsqlobject(oSql)
        loSetting = CREATEOBJECT("EMPTY")
        =loSet.LoadUserSettings(loSetting, "camp")
        IF !PEMSTATUS(loSetting, "camp", 5)
            poError = CREATEOBJECT("HtmlErrorDisplayConfig")
            poError.Errors.AddError("Not Authenticated")
            RETURN poError
        ENDIF

        RETURN loSetting.camp
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Create Item
    */--------------------------------------------------------------------------------
    PROCEDURE CampCreateItem
        LPARAMETERS lo
        pObj = CREATEOBJECT("EMPTY")
        ADDPROPERTY(pObj, "status", "Added")
        RETURN pObj
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Create Npd
    */--------------------------------------------------------------------------------
    PROCEDURE CampCreateNpd
        LPARAMETERS lo
        pObj = CREATEOBJECT("EMPTY")
        ADDPROPERTY(pObj, "status", "CREATED")
        RETURN pObj
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Create Item Test
    */--------------------------------------------------------------------------------
    PROCEDURE CampCreateItemTest
        LPARAMETERS lo
        pObj = CREATEOBJECT("EMPTY")
        ADDPROPERTY(pObj, "citemno", "blah")
        RETURN pObj
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Npd Forecast
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetNpdForecast
        LPARAMETERS lo
        RETURN 'Approved'
    ENDPROC

    */--------------------------------------------------------------------------------
    *  Camp Get Summary Vert
    */--------------------------------------------------------------------------------
    PROCEDURE CampGetSummaryVert
    ENDPROC

ENDDEFINE
