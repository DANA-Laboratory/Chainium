﻿namespace Chainium.Blockchain.Public.Node

open Chainium.Blockchain.Public.Core
open Chainium.Blockchain.Public.Core.DomainTypes
open Chainium.Blockchain.Public.Crypto
open Chainium.Blockchain.Public.Data
open Chainium.Blockchain.Public.Net

module Composition =

    // Raw storage

    let saveTx = Raw.saveTx Config.DataDir

    let getTx = Raw.getTx Config.DataDir

    let saveTxResult = Raw.saveTxResult Config.DataDir

    let getTxResult = Raw.getTxResult Config.DataDir

    let saveBlock = Raw.saveBlock Config.DataDir

    let getBlock = Raw.getBlock Config.DataDir

    let blockExists = Raw.blockExists Config.DataDir

    // DB

    let initDb () = DbInit.init Config.DbEngineType Config.DbConnectionString

    let saveTxToDb = Db.saveTx Config.DbConnectionString

    let getTxInfo = Db.getTx Config.DbConnectionString

    let getPendingTxs = Db.getPendingTxs Config.DbConnectionString

    let getTotalFeeForPendingTxs = Db.getTotalFeeForPendingTxs Config.DbConnectionString

    let getLastBlockTimestamp () = Db.getLastBlockTimestamp Config.DbConnectionString

    let getLastBlockNumber () = Db.getLastBlockNumber Config.DbConnectionString

    let getChxBalanceState = Db.getChxBalanceState Config.DbConnectionString

    let getAddressAccounts = Db.getAddressAccounts Config.DbConnectionString

    let getAccountHoldings = Db.getAccountHoldings Config.DbConnectionString

    let getHoldingState = Db.getHoldingState Config.DbConnectionString

    let getAccountState = Db.getAccountState Config.DbConnectionString

    let getAssetState = Db.getAssetState Config.DbConnectionString

    let getValidatorState = Db.getValidatorState Config.DbConnectionString

    let getAllValidators () = Db.getAllValidators Config.DbConnectionString

    let applyNewState = Db.applyNewState Config.DbConnectionString

    // Workflows

    let submitTx =
        Workflows.submitTx
            Signing.verifySignature
            Hashing.isValidChainiumAddress
            Hashing.hash
            getChxBalanceState
            getTotalFeeForPendingTxs
            saveTx
            saveTxToDb
            (ChxAmount Config.MinTxActionFee)

    let isMyTurnToProposeBlock () =
        Workflows.isMyTurnToProposeBlock
            getLastBlockNumber
            getAllValidators
            Config.ValidatorAddress

    let createNewBlock () =
        Workflows.createNewBlock
            getPendingTxs
            getTx
            Signing.verifySignature
            Hashing.isValidChainiumAddress
            getChxBalanceState
            getHoldingState
            getAccountState
            getAssetState
            getValidatorState
            getLastBlockNumber
            getBlock
            Hashing.decode
            Hashing.hash
            Hashing.merkleTree
            saveTxResult
            saveBlock
            applyNewState
            (ChxAmount Config.MinTxActionFee)
            Config.MaxTxCountPerBlock
            (ChainiumAddress Config.ValidatorAddress)

    let initBlockchainState () =
        Workflows.initBlockchainState
            getLastBlockNumber
            getBlock
            saveBlock
            applyNewState
            Hashing.decode
            Hashing.hash
            Hashing.merkleTree
            Hashing.zeroHash
            Hashing.zeroAddress
            (ChxAmount Config.GenesisChxSupply)
            (ChainiumAddress Config.GenesisAddress)
            Config.GenesisValidators

    let advanceToLastKnownBlock () =
        Workflows.advanceToLastKnownBlock
            getTx
            Signing.verifySignature
            Hashing.isValidChainiumAddress
            getChxBalanceState
            getHoldingState
            getAccountState
            getAssetState
            getValidatorState
            Hashing.decode
            Hashing.hash
            Hashing.merkleTree
            saveTxResult
            saveBlock
            applyNewState
            getLastBlockNumber
            blockExists
            getBlock
            (ChxAmount Config.MinTxActionFee)

    let propagateTx = Workflows.propagateTx Peers.sendMessage Config.NetworkAddress getTx

    let propagateBlock = Workflows.propagateBlock Peers.sendMessage Config.NetworkAddress getBlock

    // Network

    let processPeerMessage (peerMessage : PeerMessage) =
        Workflows.processPeerMessage getTx submitTx peerMessage

    let startGossip publishEvent =
        Peers.startGossip
            Transport.sendGossipDiscoveryMessage
            Transport.sendGossipMessage
            Transport.sendMulticastMessage
            Transport.receiveMessage
            Transport.closeConnection
            Config.NetworkAddress
            Config.NetworkBootstrapNodes
            getAllValidators
            processPeerMessage
            publishEvent
    // API

    let getAddressApi = Workflows.getAddressApi getChxBalanceState

    let getAddressAccountsApi = Workflows.getAddressAccountsApi getAddressAccounts

    let getAccountApi = Workflows.getAccountApi getAccountState getAccountHoldings

    let getBlockApi = Workflows.getBlockApi getBlock

    let getTxApi = Workflows.getTxApi getTx Signing.verifySignature getTxResult
