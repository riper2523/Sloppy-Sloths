export interface OwnerData {
    nick: string
}
export interface OwnerEntity {
    id: number
    data: OwnerData
}

export interface MapData {
    mapName: string
    owner: OwnerData
    filePath: string
}
export interface MapEntity {
    id: number
    owner: OwnerEntity
    data: MapData
}

export type Result<T, E> = { ok: true; value: T } | { ok: false; error: E };

export interface DatabaseConnection {
    getCurrentUser(): Promise<Result<OwnerEntity, string>>
    addUser(nick: string): Promise<Result<OwnerEntity, string>>
    getOwner(nick: string): Promise<Result<OwnerEntity, string>>

    addMap(mapName: string, owner: OwnerEntity, filePath: string): Promise<Result<MapEntity, string>>
    getMapList(): Promise<Result<MapEntity[], string>>
    deleteMap(mapName: string): Promise<Result<void, string>>

    getMap(mapName: string): Promise<Result<MapEntity, string>>
    changeOwner(mapEntity: MapEntity, newOwner: OwnerEntity): Promise<Result<MapEntity, string>>
}
